#include "DHTManaged.h"

DHTManaged::DHTManaged(uint8_t pin, uint8_t type, uint8_t count, int32_t sensorID):
  _dht(pin, type, count),
  _type(type),
  _pin(pin)
{

  switch(type) {
    case DHT11:
      sensorName = (char*)"DHT11";
      break;
    case DHT21:
      sensorName = (char*)"DHT21";
      break;
    case DHT22:
      sensorName = (char*)"DHT22";
      break;
    default:
      sensorName = (char*)"DHT?";
      break;
  }  
  this->sensorID = sensorID;
  this->setSaveSize(DHTManaged::SAVE_SIZE);
  this->setSaveID(SENSOR_DHT);
}

DHTManaged::~DHTManaged(){
  //delete[] &_dht;
}

void DHTManaged::begin() {
  _dht.begin();
}

void DHTManaged::outputData(Stream * outStream)
{
  float tmp = _dht.readTemperature();
  float hmd = _dht.readHumidity();

  char txt[50];
  sprintf(txt,"[%d]%s: %.2f, %.2f",sensorID,sensorName,tmp,hmd);
  outStream->print(txt);
}

void DHTManaged::outputParameters(Stream * outStream)
{
  char text[25];
  sprintf(text,"[%d]DHT;%d;%d;%d;",getID(),_pin,_type,getUpdateTime());
  outStream->println(text);
}


ManagedSensor* DHTManaged::loadSensor(uint16_t* data)
{
  uint8_t pin, type;
  ManagedSensor * sensor;

  pin = (data[0] >> 8) & 0xFF;
  type = (data[0]) & 0xFF;

  sensor = (ManagedSensor*) new DHTManaged(pin,type);
  sensor->begin();
  sensor->setUpdateTime((unsigned long)(data[1] * 1000));

  char txt[45];
  sprintf(txt,"NEW %d - %d - %ul",pin,type,sensor->getUpdateTime());
  Serial1.println(txt);
  
  return sensor;
}

void DHTManaged::saveSensor(uint16_t* data)
{

  *data = ((_pin&0xFF) << 8) | (_type & 0xFF);
  data++;
  // Convert miliseconds to seconds for save purposes
  uint16_t utime = getUpdateTime()/1000;
  *data = utime;
}

