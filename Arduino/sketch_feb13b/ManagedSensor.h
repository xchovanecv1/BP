#ifndef MANAGEDSENSOR_H
#define MANAGEDSENSOR_H

#if ARDUINO >= 100
 #include "Arduino.h"
#else
 #include "WProgram.h"
#endif


class ManagedSensor {
 public:
  // Constructor(s)
  ManagedSensor() {}
  virtual ~ManagedSensor() {};

  // These must be defined by the subclass
  virtual void outputData(Stream * outStream) = 0;
  virtual void saveSensor(uint16_t*) = 0;
  virtual void outputParameters(Stream * outStream);
  virtual void begin() = 0;
  
  static const uint8_t SAVE_SIZE;
  
  void setID(int32_t); 
  int32_t getID();
  void setSaveSize(uint8_t);
  uint8_t getSaveSize();
  void setSaveID(uint8_t);
  uint8_t getSaveID();

  unsigned long getUpdateTime();
  void setUpdateTime(unsigned long);

  uint8_t checkTime(unsigned long);
  
  protected:
  int32_t sensorID = -1;
  char * sensorName = 0;
  uint8_t saveSize = 0;
  uint8_t saveID = 0;
  unsigned long lastUpdate = 0;
  unsigned long updateTime = -1;
  
};


#endif

