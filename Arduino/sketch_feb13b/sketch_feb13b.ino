///https://github.com/FoxExe/STM32_RTCLib

#include <libmaple/iwdg.h>
#include <EEPROM.h>

#include "SerialCommand.h"

/*      DHT SENSORS */

#include "SensorManager.h"
#include "DHTManaged.h"
#include "DHT.h"
//#include "RTClib.h"
//#include "RTC_Buildin.h"

//RTC_Buildin rtc;

SensorManager sensorManagement(&Serial1);

uint32_t delayMS;


/**********************************************************************/

/**********************************************************************/

void command_AddDHT(uint8_t * data, struct __SerialCommand*,Stream * out);
void command_ListSens(uint8_t * data, struct __SerialCommand*,Stream * out);
void command_DelSens(uint8_t * data, struct __SerialCommand*,Stream * out);
void command_SaveSens(uint8_t * data, struct __SerialCommand*,Stream * out);
void command_LoadSens(uint8_t * data, struct __SerialCommand*,Stream * out);
void command_FlushSens(uint8_t * data, struct __SerialCommand*,Stream * out);
void command_SetSensDelay(uint8_t * data, struct __SerialCommand*,Stream * out);

void command_SetPwmDuty(uint8_t * data, struct __SerialCommand*,Stream * out);

SerialFunction commands[]={
    {"dhtadd", "pin[];DHT[(11/21/22)];updateTime[s]",FUNCTION_CRC_FORCE,command_AddDHT},
    {"list", "",FUNCTION_CRC_VAGUE,command_ListSens},
    {"del", "id[];",FUNCTION_CRC_FORCE,command_DelSens},
    {"save", "",FUNCTION_CRC_VAGUE,command_SaveSens},
    {"load", "",FUNCTION_CRC_VAGUE,command_LoadSens},
    {"flush", "",FUNCTION_CRC_VAGUE,command_FlushSens},
    {"setd", "id[];delay[UL];",FUNCTION_CRC_FORCE,command_SetSensDelay},
    {"pwms", "val[0-65535]",FUNCTION_CRC_FORCE,command_SetPwmDuty},
  };

SerialCommand serialHandler(commands,(sizeof(commands)/sizeof(SerialFunction)),&Serial1);

ManagedTypes managedType[]={
    {SENSOR_DHT,DHTManaged::SAVE_SIZE,DHTManaged::loadSensor}
  };

void command_SetPwmDuty(uint8_t * data, struct __SerialCommand * cmd,Stream * out)
{
  uint8_t paramCount = SerialCommand::getParameterCount(';',cmd);
  if(paramCount == 1)
  {
    uint8_t *param;
    uint16_t val = 0;
    
    if(!(param = SerialCommand::getParameter(';',cmd))) return;
    val = atoi((char*)param);

    pwmWrite(PB6,val);
    out->print("pwms: OK");
    
   } else {
      // Nespravny pocet parametrov
      out->print("pwms ERROR: expected 1 parameters, given ");
      out->print(paramCount,DEC);
   }
}


void command_AddDHT(uint8_t * data, struct __SerialCommand * cmd,Stream * out)
{
  uint8_t paramCount = SerialCommand::getParameterCount(';',cmd);
  if(paramCount == 3)
  {
    uint8_t *param;
    uint8_t pin = 0, type = 0, delayT = 2;
    
    if(!(param = SerialCommand::getParameter(';',cmd))) return;
    pin = atoi((char*)param);
    if(!(param = SerialCommand::getParameter(';',cmd))) return;
    type = atoi((char*)param);
    if(!(param = SerialCommand::getParameter(';',cmd))) return;
    delayT = atoi((char*)param);
  
    if(pin && type && delayT)
    {
      ManagedSensor * sn = new DHTManaged(pin,type);
      sn->setUpdateTime(delayT*1000);
      sn->begin();
      //sn->outputData();
      sensorManagement.addSensor(sn);

      out->print("dhtadd: OK");
    }
   } else {
      // Nespravny pocet parametrov
      out->print("dhtadd ERROR: expected 3 parameters, given ");
      out->print(paramCount,DEC);
   }
}

void command_SetSensDelay(uint8_t * data, struct __SerialCommand * cmd,Stream * out)
{
  uint8_t paramCount = SerialCommand::getParameterCount(';',cmd);
  if(paramCount == 2)
  {
    uint8_t *param;
    uint8_t id = 0;
    unsigned long del = 0;
    
    if(!(param = SerialCommand::getParameter(';',cmd))) return;
    id = atoi((char*)param);
    if(!(param = SerialCommand::getParameter(';',cmd))) return;
    del = strtoul ((char*)param, NULL, 0);
  
    if(del)
    {
      ManagedSensor * sn = sensorManagement.getSensor(id);
      sn->setUpdateTime(del);
      out->print("setd: OK");
    }
   } else {
      // Nespravny pocet parametrov
      out->print("setd ERROR: expected 2 parameters, given ");
      out->print(paramCount,DEC);
   }
}

void command_ListSens(uint8_t * data, struct __SerialCommand * cmd,Stream * out)
{
  out->println("list OK");
  sensorManagement.listSensors(out);
}

void command_SaveSens(uint8_t * data, struct __SerialCommand * cmd,Stream * out)
{
  sensorManagement.save();
  
  out->print("save OK");
}

void command_FlushSens(uint8_t * data, struct __SerialCommand * cmd,Stream * out)
{
  sensorManagement.flush();
  
  out->print("flush OK");
}


void command_LoadSens(uint8_t * data, struct __SerialCommand*,Stream * out)
{
  sensorManagement.flush();
  sensorManagement.load();
  out->println("load OK");
  sensorManagement.listSensors(out);
}

void command_DelSens(uint8_t * data, struct __SerialCommand * cmd,Stream * out)
{
  uint8_t paramCount = SerialCommand::getParameterCount(';',cmd);
  uint8_t id;
  uint8_t *param;
  if(paramCount == 1)
  {
    if(!(param = SerialCommand::getParameter(';',cmd))) return;
    id = atoi((char*)param);
    if(sensorManagement.deleteSensor(id))
    {
      out->print("del:");
      out->print(id,DEC);
      out->print(" OK");
    } else {
      out->print("del ERROR: invalid id");
    }
  } else {
    out->print("del ERROR: expected 1 parameters, given ");
    out->print(paramCount,DEC);
  }
}

#define EEPROM_MEM_START 0x12
void setup() {

  // Inicializacia seriovych portov
  Serial.begin(9600);
  Serial1.begin(9600);

  // Aktivovanie hodin pre CRC modul
  rcc_clk_enable(RCC_CRC);

  // Nastavenie pozicie EEPROM emulacnych pamatovych baniek
  EEPROM.PageBase0 = 0x801F000;
  EEPROM.PageBase1 = 0x801F800;
  EEPROM.PageSize  = 0x400;
  
  serialHandler.begin();

  
  sensorManagement.begin(EEPROM_MEM_START);
  sensorManagement.setTypeList(managedType,(sizeof(managedType)/sizeof(ManagedTypes)));

  // Inicializacia WatchDog timera
  iwdg_init(IWDG_PRE_256, 625); // 4 seconds
  
  // Inicializacia RTC hodin
//  rtc.begin(DateTime(F(__DATE__), F(__TIME__)));

  // Nacitanie vsetkych senzorov ulozenych v pamati EEPROM
  sensorManagement.load();

  pinMode(PB6,PWM);
  pwmWrite(PB6,0);

}

uint16 Data;
unsigned long mlsTime;

void loop() {
  // Restart pocitadla pre WatchDog
  iwdg_feed();
  
  // Zistenie aktualneho casu behu programu pre porovnavacie ucely
  mlsTime = millis();

  //Kontrola dostupnosti textu zo vstupneho zasobnika
  serialHandler.poolStream();

  // Kontrola prichadzajucich seriovych prikazov
  serialHandler.checkCommands();

  // Periodicka kontrola vykonania merania pre senzor
  sensorManagement.checkSensors(mlsTime);
  
}

