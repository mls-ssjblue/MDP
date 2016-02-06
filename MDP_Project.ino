#include <PID_v1.h>
#include <digitalWriteFast.h>
#include "PinChangeInt.h"
#include "DualVNH5019MotorShield.h"

#define DEBUG 0

//Hardware Pin Definitions -----------------------------------------------------------------------------------
int enc_a1 = 3;
int enc_a2 = 5;
int enc_b1 = 11;
int enc_b2 = 13;
int SensorIR[5] = {A2, A4, A1, A5, A3};

//IR Sensor --------------------------------------------------------------------------------------------------
#define SENSE_HISTORY 15

#define IR_SL 0
#define IR_FL 1
#define IR_F  2
#define IR_FR 3
#define IR_SR 4

int SL_TRIG = 315;
int FL_TRIG = 460;
int F_TRIG = 310;
int FR_TRIG = 460;
int SR_TRIG = 315;

//Alignment Parameters
bool ENABLE_ALIGN = true;
int AlignIgnoreRange = 8;
int AlignRange = 100;
int RAlignMid = 397; //406; //413;
int LAlignMid = 409; //416; //413;
double AlignBiaMax = 1.15;

int FL_CAL = 519;
int FR_CAL = 520;
int FORWARD_AFTER_WIGGLE = 420;

//Sensor Declarations
volatile int SenInd[5];
volatile int SenAccum[5];
volatile int SenHist[5][SENSE_HISTORY + 1];
volatile float SenAvg[5];
volatile int Sens_Roll = 1;

void SensorIR_Init()
{ byte sens, i;
  for (sens=0;sens<5;sens++)
  { pinMode(SensorIR[sens], INPUT);
    for (i=0;i<=SENSE_HISTORY;i++)
    { SenHist[sens][i] = 0;
    }
    SenAccum[sens] = 0;
    SenInd[sens] = 1;
  }
}

double SensorIR_Proc(int sensIndex)
{ SenAccum[sensIndex] -= SenHist[sensIndex][SenInd[sensIndex]];
  SenHist[sensIndex][SenInd[sensIndex]] = analogRead(SensorIR[sensIndex]);
  SenAccum[sensIndex] += SenHist[sensIndex][SenInd[sensIndex]];
  SenInd[sensIndex] = SenInd[sensIndex] % SENSE_HISTORY + 1;
  SenAvg[sensIndex] = SenAccum[sensIndex] / SENSE_HISTORY;  
  return SenAvg[sensIndex];
}

float SensorIR_Read(byte sensIndex)
{ return SenAvg[sensIndex];
}

//Timer
void Timer_Init()
{ TCCR2B = 0x00;
  TCNT2 = 155;
  TIFR2 = 0x00;
  TIMSK2 = 0x01;
  TCCR2A = 0x00;
  TCCR2B = 0x03;
}

ISR(TIMER2_OVF_vect)
{ Sens_Roll = Sens_Roll % 5 + 1;
  SensorIR_Proc(Sens_Roll - 1);
  TCNT2 = 155;
  TIFR2 = 0x00;
}

//Encoder ----------------------------------------------------------------------------------------------------
#define LEFT_ENC 1
#define RIGHT_ENC 2
#define BOTH_ENC 3

int dir_map[16] = {0, -1, 1, 0, 1, 0, 0, -1, -1, 0, 0, 1, 0, 1, -1, 0};
volatile long enc_steps[3];
volatile long enc_accum[3];
volatile long enc_abssteps[3];
volatile long enc_dir[3]; 

void Encoder_Init()
{ pinMode(enc_a1, INPUT);
  pinMode(enc_a2, INPUT);
  pinMode(enc_b1, INPUT);
  pinMode(enc_b2, INPUT);

  PCintPort::attachInterrupt(enc_b1, EncoderL_ISR, CHANGE);
  PCintPort::attachInterrupt(enc_b2, EncoderL_ISR, CHANGE);
  PCintPort::attachInterrupt(enc_a1, EncoderR_ISR, CHANGE);
  PCintPort::attachInterrupt(enc_a2, EncoderR_ISR, CHANGE);
  
  Encoder_Reset(BOTH_ENC);
  Encoder_ResetAccum(BOTH_ENC);
}

long Encoder_TotalSteps(bool absolute)
{ if (absolute == true)
    return (abs(enc_abssteps[LEFT_ENC]) + abs(enc_abssteps[RIGHT_ENC])) / 2;
  else
    return (abs(enc_steps[LEFT_ENC]) + abs(enc_steps[RIGHT_ENC])) / 2;
}

void Encoder_Reset(byte Flag)
{ if (Flag & 1)
  { enc_steps[LEFT_ENC] = 0;
    enc_dir[LEFT_ENC] = 0;
    enc_abssteps[LEFT_ENC] = 0;   
  }
  if (Flag & 2)
  { enc_steps[RIGHT_ENC] = 0;
    enc_dir[RIGHT_ENC] = 0;
    enc_abssteps[RIGHT_ENC] = 0;
  }
}

void Encoder_ResetAccum(byte Flag)
{ if (Flag & 1)
    enc_accum[LEFT_ENC] = 0;
  if (Flag & 2)
    enc_accum[RIGHT_ENC] = 0;
}

void EncoderL_ISR()
{ static byte prev_seq = 0;
  static long dir_predicted = 0;
  static long dir_predict[3] = { -1, 0, 1};
  byte Seq;
  long new_micros;
  Seq = (digitalReadFast(enc_b1) << 1) | digitalReadFast(enc_b2);
  dir_predict[1] = dir_predicted;
  dir_predicted = dir_predict[1 + dir_map[(prev_seq << 2) | Seq]];
  enc_steps[LEFT_ENC] += dir_predicted;
  enc_accum[LEFT_ENC]++;
  enc_abssteps[LEFT_ENC]++;  
  enc_dir[LEFT_ENC] = dir_predicted;
  prev_seq = Seq;
}

void EncoderR_ISR()
{ static byte prev_seq = 0;
  static long dir_predicted = 0;
  static long dir_predict[3] = { -1, 0, 1};
  byte Seq;
  long new_micros;
  Seq = (digitalReadFast(enc_a1) << 1) | digitalReadFast(enc_a2);
  dir_predict[1] = dir_predicted;
  dir_predicted = dir_predict[1 + dir_map[(prev_seq << 2) | Seq]];
  enc_steps[RIGHT_ENC] += dir_predicted;
  enc_accum[RIGHT_ENC]++;
  enc_abssteps[RIGHT_ENC]++;  
  enc_dir[RIGHT_ENC] = dir_predicted;
  prev_seq = Seq;
}

//Motion & Motor ---------------------------------------------------------------------------------------------
#define MAX_ALIGN 60
#define MIN_SPEED 120
#define MAX_SPEED 400
#define MAX_DECELSTEPS 750
#define MAX_PIVOTDECELSTEPS 300
#define PID_SYNC_INTERVAL 1

DualVNH5019MotorShield md;

long PIVOT_LEFT_OFFSET = 10;
long PIVOT_RIGHT_OFFSET = 25;
long PIVOT_BACK_OFFSET = 120;

double ENCODER_RES = 2249;
double WHEEL_DIA = 5.93; // 475; 
double WHEEL_DIST = 16.2; //15.82; //change this to calibrate rotation
double CELL_DIST = 10.15;
double STEP_DIST = PI * WHEEL_DIA / ENCODER_RES;
long CELL_STEP = CELL_DIST / STEP_DIST;
long HALF_CELL_STEP = CELL_STEP / 2;
long PIVOT90_STEP = PI * WHEEL_DIST / STEP_DIST / 4;
long PIVOT180_STEP = PIVOT90_STEP * 2;
long CURVE90_STEP = (PI * (CELL_DIST * 3 / 2 + WHEEL_DIST / 2) * 2) / 4 / STEP_DIST;
double CURVE90_RATIO = (PI * (CELL_DIST * 3 / 2 - WHEEL_DIST / 2)) / (PI * (CELL_DIST * 3 / 2 + WHEEL_DIST / 2)) - 0.15;
double ACCDEC_GRADIENT = (double)(MAX_SPEED - MIN_SPEED) / (double)HALF_CELL_STEP * 4;

long LastAccum = 0;
double sync_sp = 0, sync_in, sync_out, spd_sp = 0, cur_spd = 0;
double csync_sp = 0, csync_in, csync_out;
PID SyncPID(&sync_in, &sync_out, &sync_sp, 80, 0, 1.2, DIRECT);
PID SyncCurvePID(&csync_in, &csync_out, &csync_sp, 200, 0, 1.2, DIRECT);

void WiggleAlign()
{ int LDir = 0, RDir = 0, i;
  for (i = 0; i < 2; i++)
  {
    while(1)
    { LDir = FL_CAL - SensorIR_Read(IR_FL);
      RDir = FR_CAL - SensorIR_Read(IR_FR);
      if (abs(LDir) < 2) LDir = 0;
      if (abs(RDir) < 2) RDir = 0;
      if ((LDir == 0) && (RDir == 0)) break;
      /*
      Serial.print(LDir);
      Serial.print("  ");
      Serial.println(RDir);
      */
      if (LDir != 0) LDir = (LDir / abs(LDir)) * MIN_SPEED * 0.7;
      if (RDir != 0) RDir = (RDir / abs(RDir)) * MIN_SPEED * 0.7;
      md.setSpeeds(RDir, LDir);
    }
    Encoder_Reset(BOTH_ENC);
    Encoder_ResetAccum(BOTH_ENC);
    md.setSpeeds(MIN_SPEED, MIN_SPEED);  
    while(Encoder_TotalSteps(true) < FORWARD_AFTER_WIGGLE)
    {
    }  
    StopMove();
  }
}

void SpeedSyncPID_Init()
{ md.init();
  md.setBrakes(0, 0);
  md.setSpeeds(0, 0);
  SyncPID.SetSampleTime(PID_SYNC_INTERVAL);
  SyncPID.SetOutputLimits(-MAX_ALIGN, MAX_ALIGN);
  
  SyncCurvePID.SetSampleTime(PID_SYNC_INTERVAL);
  SyncCurvePID.SetOutputLimits(-MAX_SPEED, MAX_SPEED);
  
  SyncPID.SetMode(AUTOMATIC);
  SyncCurvePID.SetMode(AUTOMATIC);
}

double ClampSpeed(double InputSpeed, double minSpeed, double maxSpeed)
{ double CalcSpeed = abs(InputSpeed);
  if (CalcSpeed > maxSpeed) CalcSpeed = maxSpeed;
  if (CalcSpeed < minSpeed) CalcSpeed = minSpeed;
  return CalcSpeed;
}

void SpeedSyncPID_Compute(double &LMotorSpeed, double &RMotorSpeed, double SpeedRatio)
{ //SpdRatio < 0 Curve Left
  //SpdRatio > 0 Curve Right
  long NowAccum, Accum;
  double Base_spd = 0, LMtrSpd = abs(LMotorSpeed), RMtrSpd = abs(RMotorSpeed);  
  int LDir, RDir, RBias = 0, LBias = 0;  
  double SensLeft, SensRight, AlignBias = 0, LAlignBias = 0, RAlignBias = 0;
  if (LMotorSpeed < 0) LDir = -1; else LDir = 1;
  if (RMotorSpeed < 0) RDir = -1; else RDir = 1;     
  if (LMtrSpd > RMtrSpd) 
  { Base_spd = LMtrSpd;
    RBias = (RMtrSpd - LMtrSpd);
  }
  else
  { Base_spd = RMtrSpd;
    LBias = (LMtrSpd - RMtrSpd);
  }
  NowAccum = (enc_accum[LEFT_ENC] + enc_accum[RIGHT_ENC]) / 2;
  if (NowAccum - LastAccum > 0) 
  { Accum = NowAccum - LastAccum;
    LastAccum = NowAccum;    
    if (LMtrSpd == spd_sp)
      LMtrSpd = ClampSpeed(Base_spd - LBias, 1, spd_sp);
    else
    { if (LMtrSpd < spd_sp) LMtrSpd = ClampSpeed((Base_spd + ACCDEC_GRADIENT * Accum) - LBias, 1, spd_sp);
      if (LMtrSpd > spd_sp) LMtrSpd = ClampSpeed((Base_spd - ACCDEC_GRADIENT * Accum) - LBias, 1, spd_sp);      
    }
    if (RMtrSpd == spd_sp)
      RMtrSpd = ClampSpeed(Base_spd - RBias, 1, spd_sp);
    else
    { if (RMtrSpd < spd_sp) RMtrSpd = ClampSpeed((Base_spd + ACCDEC_GRADIENT * Accum) - RBias, 1, spd_sp);
      if (RMtrSpd > spd_sp) RMtrSpd = ClampSpeed((Base_spd - ACCDEC_GRADIENT * Accum) - RBias, 1, spd_sp);
    }     
  }    
  if (SpeedRatio == 0)
  { 
    if (ENABLE_ALIGN == true)
    { LAlignBias = 0;
      RAlignBias = 0;
      if (Encoder_TotalSteps(true) > CELL_STEP * 0.5)
      { SensLeft = SensorIR_Read(IR_SL);
        SensRight = SensorIR_Read(IR_SR);
        if ((SensLeft < (LAlignMid - AlignIgnoreRange)) || (SensLeft > (LAlignMid + AlignIgnoreRange)))
        { if (((SensLeft > LAlignMid)) || (SensLeft > (LAlignMid - AlignRange)))
          { LAlignBias = (LAlignMid - SensLeft) / AlignRange * AlignBiaMax;
          }
        }
        if ((SensRight < (RAlignMid - AlignIgnoreRange)) || (SensRight > (RAlignMid + AlignIgnoreRange)))
        { if (((SensRight > RAlignMid)) || (SensRight > (RAlignMid - AlignRange)))
          { RAlignBias = (SensRight - RAlignMid) / AlignRange * AlignBiaMax;
          }
        }
      }
      AlignBias = (LAlignBias + RAlignBias) / 2;
      if (AlignBias < 0)
      { enc_steps[LEFT_ENC] = enc_steps[LEFT_ENC] - abs(AlignBias) / 2;
        enc_steps[RIGHT_ENC] = enc_steps[RIGHT_ENC] + abs(AlignBias) / 2;
      }
      else
      { enc_steps[LEFT_ENC] = enc_steps[LEFT_ENC] + abs(AlignBias) / 2;
        enc_steps[RIGHT_ENC] = enc_steps[RIGHT_ENC] - abs(AlignBias) / 2;
      }  
    }
    sync_in = abs(enc_steps[LEFT_ENC]) - abs(enc_steps[RIGHT_ENC]);      
    if (SyncPID.Compute() == true);
    { RMtrSpd = ClampSpeed(LMtrSpd - sync_out, 5, MAX_SPEED);
      LMtrSpd = ClampSpeed(LMtrSpd + sync_out, 5, MAX_SPEED);
    }
    //Serial.println(sync_in);
  }
  else
  { if (SpeedRatio < 0)
      csync_in = (abs(enc_steps[LEFT_ENC]) * abs(SpeedRatio)) - abs(enc_steps[RIGHT_ENC]);
    else
      csync_in = abs(enc_steps[LEFT_ENC]) - (abs(enc_steps[RIGHT_ENC]) * abs(SpeedRatio));
    if (SyncCurvePID.Compute() == true);
    { RMtrSpd = ClampSpeed(RMtrSpd - csync_out, MIN_SPEED, MAX_SPEED);
      LMtrSpd = ClampSpeed(LMtrSpd + csync_out, MIN_SPEED, MAX_SPEED);
    }
  } 
  LMotorSpeed = LMtrSpd * LDir;
  RMotorSpeed = RMtrSpd * RDir;  
  if (LMtrSpd > RMtrSpd) cur_spd = LMtrSpd; else cur_spd = RMtrSpd;
}

//Main Code --------------------------------------------------------------------------------------------------
#define NORTH 0
#define EAST  1
#define SOUTH 2
#define WEST  3

#define RIGHT 0
#define BACK  1
#define LEFT  2

String Commands = "";
byte RobotDirection = NORTH, RobotCoordX = 1, RobotCoordY = 1;

void StopMove()
{ md.setBrakes(400,400);
  delay(250);
  md.setBrakes(0,0);
}

bool TurnPivot(int TurnType, byte &CurDir)
{ double LSpeed, RSpeed;
  long dst, cur_step = 0, DecelSteps = MAX_PIVOTDECELSTEPS, AccelSteps = dst - DecelSteps;
  long LMinSpd, RMinSpd;
  int NewDir = CurDir;
  String CommandData = "";
  switch(TurnType)
  { case RIGHT:
      dst = PIVOT90_STEP + PIVOT_RIGHT_OFFSET;
      LSpeed = MIN_SPEED;
      RSpeed = -MIN_SPEED;
      LMinSpd = MIN_SPEED;
      RMinSpd = -MIN_SPEED; 
      NewDir++;
      if (NewDir > 3) NewDir = 0;
      break;
    case LEFT:
      dst = PIVOT90_STEP + PIVOT_LEFT_OFFSET;    
      LSpeed = -MIN_SPEED;
      RSpeed = MIN_SPEED;
      LMinSpd = -MIN_SPEED;
      RMinSpd = MIN_SPEED;   
      NewDir--;
      if (NewDir < 0) NewDir = 3;
      break;
    case BACK:
      dst = PIVOT180_STEP + PIVOT_BACK_OFFSET;    
      LSpeed = MIN_SPEED;
      RSpeed = -MIN_SPEED;
      LMinSpd = MIN_SPEED;
      RMinSpd = -MIN_SPEED;
      NewDir+=2;
      if (NewDir > 3) NewDir = NewDir - 4;      
      break;
  }  
  CurDir = NewDir;
  LastAccum = 0;
  Encoder_Reset(BOTH_ENC);
  Encoder_ResetAccum(BOTH_ENC); 
  spd_sp = MAX_SPEED;  
  do
  { cur_step = Encoder_TotalSteps(true);   
    if ((cur_step > dst - MAX_PIVOTDECELSTEPS) && (cur_step < dst))
       spd_sp = MIN_SPEED;
    else
    { if (cur_step < AccelSteps) 
        spd_sp = MAX_SPEED; 
    }
    SpeedSyncPID_Compute(LSpeed, RSpeed, 0);
    if (abs(LSpeed) < abs(LMinSpd)) LSpeed = LMinSpd;
    if (abs(RSpeed) < abs(RMinSpd)) RSpeed = RMinSpd;
    md.setSpeeds((int)RSpeed, (int)LSpeed);   
  } while(Encoder_TotalSteps(true) < dst);  
  StopMove();
  return true;
}

String GenerateCoordInfo(byte CoordX, byte CoordY, byte Dir, byte SL, byte FL, byte F, byte FR, byte SR, byte CellUpdateFlag)
{ String CoordInfo = "";
  if (CellUpdateFlag == 1)
    CoordInfo.concat(1);
  else
    CoordInfo.concat(2);
  if (CoordX < 10) CoordInfo.concat("0");
  CoordInfo.concat(CoordX);
  if (CoordY < 10) CoordInfo.concat("0");
  CoordInfo.concat(CoordY);
  CoordInfo.concat(Dir);
  if (CellUpdateFlag == 1)
  { CoordInfo.concat(SL);
    CoordInfo.concat(FL);
    CoordInfo.concat(F);
    CoordInfo.concat(FR);
    CoordInfo.concat(SR);
  }
  else
    CoordInfo.concat("00000");
  CoordInfo.concat("Z");
  return CoordInfo;
}

bool MapForward(int Cells, bool ProcessBlocked, bool BrakeOnEnd, byte CurDir, byte &CurCoordX, byte &CurCoordY, bool Fastrun)
{ int i, c, cell_offset = 0, last_cell_offset = 0;
  double cell_pos, LSpeed = MIN_SPEED, RSpeed = MIN_SPEED;
  bool Blocked = false, LastBlocked = false;
  long dst = Cells * CELL_STEP, cur_step = 0, DecelSteps = MAX_DECELSTEPS, AccelSteps = dst - DecelSteps;
  byte LEFT_BLOCK = 0, RIGHT_BLOCK = 0, FRONTL_BLOCK = 0, FRONT_BLOCK = 0, FRONTR_BLOCK = 0;
  byte EndX = CurCoordX, EndY = CurCoordY;  
  String CommandData = "";
  for (i = 0; i < Cells; i++)
    UpdateRobotCoord(CurDir, EndX, EndY);
  
  if (SensorIR_Read(IR_SL) > SL_TRIG) LEFT_BLOCK = 1;       
  if (SensorIR_Read(IR_SR) > SR_TRIG) RIGHT_BLOCK = 1;
  if (SensorIR_Read(IR_FL) > FL_TRIG) { FRONTL_BLOCK = 1; Blocked = true; }
  if (SensorIR_Read(IR_F) > F_TRIG)   { FRONT_BLOCK = 1; Blocked = true; }
  if (SensorIR_Read(IR_FR) > FR_TRIG) { FRONTR_BLOCK = 1; Blocked = true; }
  
  if ((FRONTL_BLOCK + FRONT_BLOCK + FRONTR_BLOCK) == 3) 
  { WiggleAlign();
  }
 
  //Send to algo current coordinate obstacle info (original cell, no movement yet)
  Serial.println(GenerateCoordInfo(CurCoordX, CurCoordY, CurDir, LEFT_BLOCK, FRONTL_BLOCK, FRONT_BLOCK, FRONTR_BLOCK, RIGHT_BLOCK, 1));
  
  if (Blocked == false)
  { LEFT_BLOCK = 0;
    FRONTL_BLOCK = 0;
    FRONT_BLOCK = 0;
    FRONTR_BLOCK = 0;
    RIGHT_BLOCK = 0;
    LastAccum = 0;
    md.setSpeeds(LSpeed, RSpeed);
    Encoder_Reset(BOTH_ENC);
    Encoder_ResetAccum(BOTH_ENC); 
    spd_sp = MAX_SPEED; 
    do
    { cur_step = Encoder_TotalSteps(true);
      cell_pos = (double)(cur_step % CELL_STEP) / (double)CELL_STEP * 100;    
      cell_offset = (cur_step - (cur_step % CELL_STEP)) / CELL_STEP;
    
      if ((cell_offset > last_cell_offset) && (Blocked == false))
      { UpdateRobotCoord(CurDir, CurCoordX, CurCoordY);
        if (Fastrun == false)
        { //Send to algo previous coordinate obstacle info
          Serial.println(GenerateCoordInfo(CurCoordX, CurCoordY, CurDir, LEFT_BLOCK, FRONTL_BLOCK, FRONT_BLOCK, FRONTR_BLOCK, RIGHT_BLOCK, 1));
        }
        LEFT_BLOCK = 0;
        FRONTL_BLOCK = 0;
        FRONT_BLOCK = 0;
        FRONTR_BLOCK = 0;
        RIGHT_BLOCK = 0;
        last_cell_offset = cell_offset;
      }
      if ((cell_pos > 95) && (cell_pos < 99))
      { if (SensorIR_Read(IR_SL) > SL_TRIG) LEFT_BLOCK = 1;   
        if (SensorIR_Read(IR_SR) > SR_TRIG) RIGHT_BLOCK = 1;
      }
      if ((cell_pos > 30) && (cell_pos < 90))
      { LastBlocked = false;
        if (SensorIR_Read(IR_FL) > FL_TRIG) { FRONTL_BLOCK = 1; LastBlocked = true; }
        if (SensorIR_Read(IR_F) > F_TRIG)   { FRONT_BLOCK = 1; LastBlocked = true; }
        if (SensorIR_Read(IR_FR) > FR_TRIG) { FRONTR_BLOCK = 1; LastBlocked = true; }        
        if ((Blocked == false) && (LastBlocked == true)) 
        { Blocked = true; 
          if (ProcessBlocked == true)
             dst = Encoder_TotalSteps(true) + MAX_DECELSTEPS;
        }
      }
      if ((cur_step > dst - MAX_DECELSTEPS) && (cur_step < dst))
         spd_sp = MIN_SPEED;
      else
      { if (cur_step < AccelSteps) 
          spd_sp = MAX_SPEED; 
      }
      SpeedSyncPID_Compute(LSpeed, RSpeed, 0);  
      md.setSpeeds((int)RSpeed, (int)LSpeed);
    } while(cur_step < dst);
    if (BrakeOnEnd == true)
      StopMove();
    
    if (Blocked == true)
    { UpdateRobotCoord(CurDir, CurCoordX, CurCoordY);
      Serial.println(GenerateCoordInfo(CurCoordX, CurCoordY, CurDir, LEFT_BLOCK, FRONTL_BLOCK, FRONT_BLOCK, FRONTR_BLOCK, RIGHT_BLOCK, 1));
      if ((FRONTL_BLOCK + FRONT_BLOCK + FRONTR_BLOCK) == 3) WiggleAlign();
    }
  }  
  if ((EndX == CurCoordX) && (EndY == CurCoordY))
    return true;
  else
    return false;
}

void UpdateRobotCoord(byte CurDir, byte &CurCoordX, byte &CurCoordY)
{ switch(CurDir)
  { case NORTH:
      CurCoordY++;
      break;
    case EAST:
      CurCoordX++;
      break;      
    case SOUTH:
      CurCoordY--;
      break;
    case WEST:
      CurCoordX--;
      break;            
  }
}

void InitializePosition(int CoordX, int CoordY, byte CurDir)
{ RobotCoordX = CoordX;
  RobotCoordY = CoordY;
  RobotDirection = CurDir;
}

void setup()
{ Serial.begin(9600);
  Encoder_Init();
  SensorIR_Init();
  Timer_Init();
  SpeedSyncPID_Init();    
  InitializePosition(1, 1, NORTH);
  
  //delay(2000);
  //MapForward(17, false, true, RobotDirection, RobotCoordX, RobotCoordY, false);
  //MapForward(int Cells, bool ProcessBlocked, bool BrakeOnEnd, byte CurDir, byte &CurCoordX, byte &CurCoordY, bool Fastrun)
}

String CheckSerial()
{ char inData;
  String CompleteCommand = "";
  while(Serial.available())
  { inData = Serial.read();
    if (((inData >= 65) && (inData <= 90)) || ((inData >= 48) && (inData <= 57)))
    { if (inData == 'Z')
      { CompleteCommand = Commands;
        Commands = "";
        break;
      }
      else
        Commands.concat(inData);
    }
  }
  return CompleteCommand;
}


String SvrCommandData = "FS17R12L00Z";
long last_time;

void loop() {
  int i, CellsMove;
  char Cmd;
  String Magnitude;
  
  //for (i = 0; i < 5; i++)
  //  SensorIR_Proc(i);    
    
  /* Valid Commands
     T       --> 'T' Reset Robot and start from beginning, end of command is a new line character
     FS17R12 --> 'F' Denotes "Follow path", "S17" means STRAIGHT 17, "R12" means "RIGHT 12", end of command is a new line character
  */
  /*
  Serial.print(SensorIR_Read(IR_SL));
  Serial.print("  ");
  Serial.println(SensorIR_Read(IR_SR));
  */
  SvrCommandData = CheckSerial();
  if (SvrCommandData.length() > 0)
  { //Serial.println(SvrCommandData);
    //Serial.println(SvrCommandData.length());
    switch(SvrCommandData.charAt(0))
    { case 'T': //Abrupt Reset
        if (SvrCommandData.length() > 1)
        { switch(SvrCommandData.charAt(1))
          { case 'W':
              WiggleAlign();
            case 'C':
              Magnitude = "";
              Magnitude.concat(SvrCommandData.charAt(2));
              Magnitude.concat(SvrCommandData.charAt(3));
              RobotCoordX = Magnitude.toInt();
              Magnitude = "";
              Magnitude.concat(SvrCommandData.charAt(4));
              Magnitude.concat(SvrCommandData.charAt(5));
              RobotCoordY = Magnitude.toInt();
              Magnitude = "";
              Magnitude.concat(SvrCommandData.charAt(6));
              RobotDirection = Magnitude.toInt();
            break;
          }
        }
        else
        { StopMove();
          InitializePosition(1, 1, NORTH);
          Serial.println(GenerateCoordInfo(RobotCoordX, RobotCoordY, RobotDirection, 0, 0, 0, 0, 0, 0));
        }
        break;
      case 'F': //Follow Search Path
        for (i = 2; i < SvrCommandData.length(); i+=3)
        { switch(SvrCommandData.charAt(i))
          { case 'S':
              break;
            case 'R':
              TurnPivot(RIGHT, RobotDirection);
              break;        
            case 'L':
              TurnPivot(LEFT, RobotDirection);        
              break;        
            case 'B':
              TurnPivot(BACK, RobotDirection);
              break;        
          }
          Magnitude = "";
          Magnitude.concat(SvrCommandData.charAt(i + 1));
          Magnitude.concat(SvrCommandData.charAt(i + 2));
          CellsMove = Magnitude.toInt();
          if (CellsMove > 0)
          { if (MapForward(CellsMove, true, true, RobotDirection, RobotCoordX, RobotCoordY, false) == false)
            { break;
            }
          }
        }
        //Send command to request new action;
        Serial.println(GenerateCoordInfo(RobotCoordX, RobotCoordY, RobotDirection, 0, 0, 0, 0, 0, 0));
        break;
    }
    SvrCommandData = "";
  }
  /*
  if (millis() - last_time > 50)
  { last_time = millis();
    if (SensorIR_Read(IR_SL) > 270) Serial.print("1  "); else Serial.print("0  ");
    if (SensorIR_Read(IR_FL) > 480) Serial.print("1  "); else Serial.print("0  ");
    if (SensorIR_Read(IR_F) > 350) Serial.print("1  "); else Serial.print("0  ");
    if (SensorIR_Read(IR_FR) > 480) Serial.print("1  "); else Serial.print("0  ");
    if (SensorIR_Read(IR_SR) > 270) Serial.print("1  "); else Serial.print("0  ");
    Serial.println("");
  }
  if (millis() - last_time > 50)
  { 
    last_time = millis();
    */
    
    //Serial.print(SensorIR_Read(IR_SL));
    //Serial.print("   ");
    //Serial.print(SensorIR_Read(IR_FL));
    //Serial.print("   ");
    //Serial.print(SensorIR_Read(IR_F));
    //Serial.print("   ");
    //Serial.println(SensorIR_Read(IR_FR));
    //Serial.print("   ");
    //Serial.println(SensorIR_Read(IR_SR));
    
  //}
}
