#ifndef SENSORMANAGER_H
#define SENSORMANAGER_H

#if ARDUINO >= 100
 #include "Arduino.h"
#else
 #include "WProgram.h"
#endif

#include "ManagedSensor.h"

typedef enum SensorTypes{
      SENSOR_NONE = 0,
      SENSOR_DHT  = 1  
} SensorTypes;

typedef struct SensorList {
  ManagedSensor *sensor;
  SensorList *next = 0;
} SensorList;

typedef struct ManagedTypes{
  SensorTypes id;   // Unique identifier for memmory save
  uint8_t size; // Number of EEPROM blocks reserved for data
  ManagedSensor* (*function)(uint16_t*);  
} ManagedTypes;

class SensorManager {
 public:
  // Constructor(s)
  SensorManager();
  SensorManager(Stream * IOstream);
  virtual ~SensorManager() {}

  void begin(uint16_t addr);
  void setTypeList(ManagedTypes *types,uint8_t count);
  void addSensor(ManagedSensor *sensor);
  ManagedSensor *getSensor(uint8_t id);
  uint8_t deleteSensor(uint8_t id);
  void listSensors(Stream * outStream);
  void deleteFromList(SensorList ** first,uint8_t pos);
  void save();
  void load();
  void flush();

  void checkSensors(unsigned long);

  private:
    ManagedTypes* _managedTypes = 0;
    SensorList *__managedSensors = 0;
    uint8_t typesCount;

    uint16_t eepromAddress = 0;
    uint16_t sensorCount = 0;

    uint16_t actualSensor = 0;

    ManagedTypes* findType(uint8_t id);
    Stream * IOstream;
  
};





#endif

