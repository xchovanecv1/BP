#ifndef DHTManagedU_H
#define DHTManagedU_H

#include "ManagedSensor.h"
#include "SensorManager.h"
#include "DHT.h"

class DHTManaged : public ManagedSensor{
public:
  DHTManaged(uint8_t pin, uint8_t type, uint8_t count=6, int32_t sensorID=-1);
  ~DHTManaged();
  void begin();
  void outputData(Stream * outStream);
  void saveSensor(uint16_t* data);
  void outputParameters(Stream * outStream);
 
  static const uint8_t SAVE_SIZE = 2;
  
  static ManagedSensor* loadSensor(uint16_t*);

  private:
  DHT _dht;
  uint8_t _type;
  uint8_t _pin;
};

#endif

