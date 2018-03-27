#include "ManagedSensor.h"

void ManagedSensor:: setID(int32_t id)
{
  sensorID = id;
}

void ManagedSensor:: outputParameters(Stream * outStream)
{
  
}

int32_t ManagedSensor:: getID()
{
  return sensorID;
}

void ManagedSensor:: setSaveSize(uint8_t size)
{
  saveSize = size;
}

uint8_t ManagedSensor:: getSaveSize()
{
  return saveSize;
}

void ManagedSensor:: setSaveID(uint8_t id)
{
  saveID = id;
}

uint8_t ManagedSensor:: getSaveID()
{
  return saveID;
}

uint8_t ManagedSensor:: checkTime(unsigned long actual)
{
  if((actual - lastUpdate) > updateTime)
  {
    lastUpdate = actual;
    return 1;
  }
  return 0;
}

unsigned long ManagedSensor:: getUpdateTime()
{
  return updateTime;
}

void ManagedSensor:: setUpdateTime(unsigned long time)
{
  updateTime = time;
}


