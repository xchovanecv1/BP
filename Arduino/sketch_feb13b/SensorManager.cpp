#include "SensorManager.h"
#include <EEPROM.h>

SensorManager::SensorManager() {

}

SensorManager::SensorManager(Stream * stream) {
  IOstream = stream;
}


void SensorManager::begin(uint16_t EEPROMAddr)
{
  eepromAddress = EEPROMAddr;
}


void SensorManager::setTypeList(ManagedTypes *types, uint8_t count)
{
  _managedTypes = types;
  typesCount = count;
}

void SensorManager:: addSensor(ManagedSensor *snsr)
{
  SensorList *buf = (SensorList*)malloc(sizeof(SensorList));
  buf->next = 0;
  
  snsr->setID(sensorCount);
  sensorCount++;
  
  buf->sensor = snsr;

  if (__managedSensors)
  {
    buf->next = __managedSensors;
    __managedSensors = buf;
  } else {
    __managedSensors = buf;
  }
}


ManagedSensor *SensorManager::getSensor(uint8_t id)
{
  SensorList *sensor = __managedSensors; 
  if(id < sensorCount)
  {
    while(sensor)
    {
      if(sensor->sensor->getID() == id)
      {
        return sensor->sensor;
      }
      sensor = sensor->next;
    }
  }
  return 0;
}

void SensorManager::listSensors(Stream * outStream)
{
  ManagedSensor *sensor;
  for(uint8_t id=0; id < sensorCount; id++)
  {
    sensor = getSensor(id);
    if(sensor)
    {
      sensor->outputParameters(outStream);
    }
  }
}

void SensorManager::deleteFromList(SensorList **head_ref, uint8_t position)
{
   if (*head_ref == NULL)
      return;
 
   // Store head node
   SensorList* temp = *head_ref;
 
    // If head needs to be removed
    if (position == 0)
    {
        *head_ref = temp->next;   // Change head
        free(temp);               // free old head
        return;
    }
 
    // Find previous node of the node to be deleted
    for (int i=0; temp!=NULL && i<position-1; i++)
         temp = temp->next;
 
    // If position is more than number of ndoes
    if (temp == NULL || temp->next == NULL)
         return;
 
    // Node temp->next is the node to be deleted
    // Store pointer to the next of node to be deleted
    SensorList *next = temp->next->next;
 
    // Unlink the node from linked list
    free(temp->next);  // Free memory
 
    temp->next = next;  // Unlink the deleted node from list
}

uint8_t SensorManager:: deleteSensor(uint8_t id)
{
  SensorList *buf = __managedSensors;
  ManagedSensor * sns;
  uint8_t pos = 0;
  if (buf)
  {
    while (buf)
    {
      sns = (buf->sensor);
      if ((sns)->getID() == id)
      {
        deleteFromList(&__managedSensors,pos);
        delete sns;
        
        return 1;
      }
      buf = buf->next;
      pos++;
    }
  }
  return 0;
}

void SensorManager::save()
{
  SensorList *buf = __managedSensors;
  ManagedSensor *sns;
  uint16_t eppAddr = eepromAddress;
  while (buf)
  {
    sns = buf->sensor;
    char test[25];
    uint16_t * saveData = (uint16_t *)malloc(sns->getSaveSize() * sizeof(uint16_t));
    sns->saveSensor(saveData);

    EEPROM.write(eppAddr, sns->getSaveID());
    eppAddr++;
    for (uint8_t i = 0; i < sns->getSaveSize(); i++)
    {
      EEPROM.write(eppAddr, saveData[i]);
      eppAddr++;
    }


    sprintf(test, "%d - %d - %d - %d", sns->getID(), sns->getSaveSize(), *saveData, sns->getSaveID());
    free(saveData);

    Serial.println(test);
    buf = buf->next;
  }

  EEPROM.write(eppAddr, 0); // End save data with 0
}

void SensorManager::flush()
{
  for(uint8_t id=0; id < sensorCount; id++)
  {
    deleteSensor(id);  
  }
  sensorCount = 0;
}

ManagedTypes* SensorManager::findType(uint8_t id)
{
  if (_managedTypes) {
    for (uint8_t i = 0; i < typesCount; i++)
    {
      if ((_managedTypes[i]).id == id)
      {
        return &(_managedTypes[i]);
      }
    }
  }
  return 0;
}
void SensorManager::load()
{
  uint16 Data;
  uint16 *LoadData;
  uint16_t eppAddr = eepromAddress;
  uint16  Status;
  uint8_t snsID = 0;

  ManagedTypes *type;
  ManagedSensor * buff;

  // Read was succesful, and Data contains valid type ID > 0
  while (!(EEPROM.read(eppAddr, &Data)))
  {
    eppAddr++;
    if (Data)
    {
      type = findType(Data);
      if (type)
      {
        LoadData = (uint16_t *)malloc(type->size * sizeof(uint16_t));
        for (uint8_t i = 0; i < type->size; i++)
        {
          EEPROM.read(eppAddr, &(LoadData[i]));
          eppAddr++;
        }
        buff = (*(type->function))(LoadData);
        if (buff)
        {
          addSensor(buff);
        }
        
        free(LoadData);
      }
    }
  }

}

void SensorManager::checkSensors(unsigned long time)
{
  ManagedSensor *sensor;
  // No sensors to check
  if(!sensorCount) return;
  if(actualSensor >= sensorCount) actualSensor = 0;

  sensor = getSensor(actualSensor);
  if(sensor)
  {
    if(sensor->checkTime(time))
    {
      sensor->outputData(IOstream);
      IOstream->println("*");
    }
  }
  actualSensor++;
}


