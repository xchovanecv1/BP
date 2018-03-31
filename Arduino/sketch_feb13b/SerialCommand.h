#ifndef SERIALCOMMAND_H
#define SERIALCOMMAND_H

#if ARDUINO >= 100
 #include "Arduino.h"
#else
 #include "WProgram.h"
#endif

#include "CRC.h"

#define SERIAL_COMMAND_LEN 10
#define SERIAL_DATA_LEN 50

#define SERIAL_COMMAND_HELLO  "hi"
#define SERIAL_COMMAND_CRC    "crc"
#define SERIAL_COMMAND_HELP   "help"

typedef struct __SerialCommand{
  uint8_t command[SERIAL_COMMAND_LEN];
  uint8_t data[SERIAL_DATA_LEN];
  uint8_t dataLen;
  uint8_t done;
  uint8_t stage;
  uint32_t CRCin;
  uint32_t CRCdata;
  uint8_t CRCvalid;
  uint8_t started;
  uint8_t pos;
 // __SerialCommand * next;
} __SerialCommand;

typedef enum FunctionCRC
{
    FUNCTION_CRC_VAGUE = 0,
    FUNCTION_CRC_FORCE = 1
} FunctionCRC;


typedef struct SerialFunction{
  const char * command;
  const char * help;
  FunctionCRC forceCRC;   // Zahodenie dat v pripade ze je povolene CRC a prijate CRC sa nezhoduje
  void (*function)(uint8_t *, struct __SerialCommand*,Stream * out);  
} SerialFunction;


class SerialCommand{

  public: 
    SerialCommand(SerialFunction*,uint8_t,Stream*);
    void begin();
    uint8_t receiveData(uint8_t d);
    uint8_t checkCommands();
    void poolStream();
    static uint8_t *getParameter(char separator, struct __SerialCommand* cmd);
    static uint8_t getParameterCount(char separator, struct __SerialCommand* cmd);

    
    char SERIAL_BUFFER_SEPARATOR = ';';
    char SERIAL_CRC_ENABLED = 0;
    
    Stream *IOstream;

  private:
    uint8_t parseHex(uint8_t d);
    uint32_t parseCrc(uint8_t data,uint32_t * val);
    void command_Hello(uint8_t * data, struct __SerialCommand * cmd);
    void command_CRC(uint8_t *data, struct __SerialCommand * cmd);
    void command_Help(uint8_t *data, struct __SerialCommand * cmd);

    __SerialCommand* actualBuffer = NULL;
    __SerialCommand* doneBuffer = NULL;
    __SerialCommand* buffBuffer = NULL;

    SerialFunction* serialFunctions;
    uint8_t functionCount;
      
};



#endif
