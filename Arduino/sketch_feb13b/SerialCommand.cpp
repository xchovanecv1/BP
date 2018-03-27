#include "SerialCommand.h"

SerialCommand::SerialCommand(SerialFunction * commands, uint8_t count, Stream * iostr){

    serialFunctions = commands;
    functionCount = count;
    IOstream = iostr;
}

uint8_t SerialCommand::receiveData(uint8_t data)
{
  if(actualBuffer == NULL) begin();

  uint8_t* dataPoint = NULL;
  uint8_t sizePoint = 0;
  uint8_t crcData;
  uint8_t err = 0;

  // Prechadzame do dalsej sekcie prikazu, treba zmenit stage
  if(data == SERIAL_BUFFER_SEPARATOR)
  {
    switch(actualBuffer->stage)
    {
      // Faza 0, prikaz bol nacitany, prechadzam bud na CRC alebo DATA, podla nastavenia
      case 0:
        if(SERIAL_CRC_ENABLED)
        {
          actualBuffer->stage = 1;
          actualBuffer->pos = 0;
        }
        else  // Preskocim crc a naplnam rovno data
        {
          actualBuffer->stage = 2;
          actualBuffer->pos = 0;
        }
        return 1; // Preskocime pridanie separatora do dat
      break;

      case 1:
        actualBuffer->stage = 2;
        actualBuffer->pos = 0;
        crcReset();   // Restartujeme CRC buffer
        return 1; // preskocime patsovaci znak
      break;
    }
  }
  
  switch(actualBuffer->stage)
  {
    // Nacitavanie prikazu
    case 0:
      dataPoint = &(actualBuffer->command[0]);
      sizePoint = SERIAL_COMMAND_LEN-1;
    break;

    case 1:
        sizePoint = 9;
        if(actualBuffer->pos >= 8) err = 2; // CRC je prilis dlhe
        else {
          if(parseCrc(data,&(actualBuffer->CRCin)) == 0xFF)  // Chybny znak pri parsovani crc, neni HEX
            err = 1;
          else{
            actualBuffer->pos++;
            return 1; // Spravne spracovane, mozme ukoncit
          }
        }
    break;

    case 2:
      dataPoint = &(actualBuffer->data[0]);
      sizePoint = SERIAL_DATA_LEN-1;
    break;
  }

  // Data neboli prijate v spravonom formate, rusim celu operaciu a mazem data

 if(actualBuffer->pos >= sizePoint || err > 0)
  {
    IOstream->println("[ERR]Invalid format!");
    free(actualBuffer);
    actualBuffer = NULL;
    return 0;
  }
 
  
  if(actualBuffer->done == 0)
  {
    if(data != '\n' && data != '\r')
    {
      actualBuffer->started = 1;
      dataPoint[actualBuffer->pos] = data;
      actualBuffer->pos++;
      // Priebezne pocitanie CRC32 prichadzajucich dat 
      if(SERIAL_CRC_ENABLED)
      {
        CRC_BASE->DR = (uint32_t)data;
        actualBuffer->CRCdata = (CRC_BASE->DR);
        //Serial.print("+CRC:");
        //Serial.print(data);
        //Serial.print(actualBuffer->CRCdata,HEX);
        //Serial.println(";");
      }
    } else {
      // Kontrola ci prisli nejake data, v pripade ze nie ignorujeme zdvojene ukoncovacie znaky \r\n
      if(actualBuffer->started)
      {
        actualBuffer->dataLen = actualBuffer->pos;
        actualBuffer->data[actualBuffer->pos] = '\0';
        actualBuffer->done = 1;

        uint8_t* command = &(actualBuffer->command[0]);
        uint8_t* datas = &(actualBuffer->data[0]);
        
        if(SERIAL_CRC_ENABLED && actualBuffer->CRCdata == actualBuffer->CRCin)
        {
          actualBuffer->CRCvalid = 1;
        }
        doneBuffer = actualBuffer;
        actualBuffer = NULL;
      }
    }
  }
  return 1;
}

uint8_t SerialCommand::checkCommands()
{
  char* command;
  if(doneBuffer != NULL)
  {
    command = (char*)&(doneBuffer->command[0]);
    if(strcmp(SERIAL_COMMAND_HELLO,command) == 0)
    {
      command_Hello(&(doneBuffer->data[0]),doneBuffer);
      IOstream->println("*"); // End data block
    } else if(strcmp(SERIAL_COMMAND_HELLO,command) == 0)
    {
      command_CRC(&(doneBuffer->data[0]),doneBuffer);
      IOstream->println("*"); // End data block
    } else if(strcmp(SERIAL_COMMAND_HELP,command) == 0){
      command_Help(&(doneBuffer->data[0]),doneBuffer);
      IOstream->println("*"); // End data block
    }else {
      for(uint8_t i=0; i < functionCount; i++)
      {
        if(strcmp((char*)&(serialFunctions[i].command[0]),command) == 0)
        {
          if(serialFunctions[i].forceCRC == FUNCTION_CRC_FORCE)
          {
            if(SERIAL_CRC_ENABLED) // Funkcia vyzaduje validne data v pripade ze je povolena CRC validacia
            {
              if( doneBuffer->CRCvalid)
              {
                (*(serialFunctions[i].function))(&(doneBuffer->data[0]),doneBuffer,IOstream);
                IOstream->println("*"); // End data block
              }
            } else {
              (*(serialFunctions[i].function))(&(doneBuffer->data[0]),doneBuffer,IOstream);
                IOstream->println("*"); // End data block
            }
          } else {
            (*(serialFunctions[i].function))(&(doneBuffer->data[0]),doneBuffer,IOstream);
                IOstream->println("*"); // End data block
          }
          
        }
      }
    }
    free(doneBuffer);
    doneBuffer = NULL;
  }
}

void SerialCommand::begin()
{
  actualBuffer = new __SerialCommand{};
  actualBuffer->done = 0;
  actualBuffer->dataLen = 0;
  actualBuffer->pos = 0;
  actualBuffer->stage = 0;
  actualBuffer->CRCin = 0;
  actualBuffer->CRCdata = 0;
  actualBuffer->CRCvalid = 0;
  actualBuffer->started = 0;
  actualBuffer->next = NULL;

}

uint8_t SerialCommand::parseHex(uint8_t data)
{
  uint8_t out = 0xFF; //ERROR
  switch((char)data)
  {
    case '0': out = 0; break;
    case '1': out = 1; break;
    case '2': out = 2; break;
    case '3': out = 3; break;
    case '4': out = 4; break;
    case '5': out = 5; break;
    case '6': out = 6; break;
    case '7': out = 7; break;
    case '8': out = 8; break;
    case '9': out = 9; break;
    case 'a':
    case 'A': out = 0xA; break;
    case 'b':
    case 'B': out = 0xB; break;
    case 'c':
    case 'C': out = 0xC; break;
    case 'd':
    case 'D': out = 0xD; break;
    case 'e':
    case 'E': out = 0xE; break;
    case 'f':
    case 'F': out = 0xF; break;
  }

  return out;
}

uint32_t SerialCommand::parseCrc(uint8_t data,uint32_t * val)
{
  if(*val != 0) *val = *val << 4;
  uint8_t pars = parseHex(data);
  
  if(pars != 0xFF)
  {
    *val |= pars;
  }
  return pars;
}

void SerialCommand::command_Hello(uint8_t * data, struct __SerialCommand * cmd)
{
  /*      STM32F103 Device UUID*/
  uint16 *flashSize = (uint16 *) (0x1FFFF7E0);
  uint16 *idBase0 =  (uint16 *) (0x1FFFF7E8);
  uint16 *idBase1 =  (uint16 *) (0x1FFFF7E8+0x02);
  uint32 *idBase2 =  (uint32 *) (0x1FFFF7E8+0x04);
  uint32 *idBase3 =  (uint32 *) (0x1FFFF7E8+0x08);
  
  IOstream->print("hi;");
  IOstream->print(*(idBase0),HEX);
  IOstream->print(*(idBase1),HEX);
  IOstream->print(*(idBase2),HEX);
  IOstream->print(*(idBase3),HEX);  
  IOstream->print(";");
  IOstream->print(*flashSize );  
  IOstream->print(";");
  if(SERIAL_CRC_ENABLED)
  {
    IOstream->print("+CRC32");
  }
  
  IOstream->print(";");
}

void SerialCommand::command_Help(uint8_t * data, struct __SerialCommand * cmd)
{
  if(functionCount)
  {
    for(uint8_t i=0; i < functionCount; i++)
    {
      IOstream->print(serialFunctions[i].command);    
      IOstream->print(';');
      IOstream->println(serialFunctions[i].help);
    }
  }
}

void SerialCommand::command_CRC(uint8_t *data, struct __SerialCommand * cmd)
{
  if(SERIAL_CRC_ENABLED)
  {
    SERIAL_CRC_ENABLED = 0;
    IOstream->print("CRC: OK OFF");
  } else {
    SERIAL_CRC_ENABLED = 1;
    IOstream->print("CRC: OK ON");
  }
}

uint8_t *SerialCommand::getParameter(char separator, struct __SerialCommand* cmd){
  uint8_t cnt = 0;
  uint8_t *start = &(cmd->data[0]);
 
  for(uint8_t i = 0; i < SERIAL_DATA_LEN-1; i++)
  {
    if((cmd->data)[i] == separator)
    {
       (cmd->data)[i] = 0; // create \0 for parameter at the separator pos
       return start;
    }

    if((cmd->data)[i] == 0)
    {
      start = &(cmd->data[i+1]);
      continue;
    }
  }
  return 0;  
}

/*
  Alway call this function before you start handling parameters with getParameter.
*/
uint8_t SerialCommand::getParameterCount(char separator, struct __SerialCommand* cmd){
  uint8_t cnt = 0;
  uint8_t *start = &(cmd->data[0]);
 
  for(uint8_t i = 0; i < SERIAL_DATA_LEN-1; i++)
  {
    if((cmd->data)[i] == separator)
    {
       cnt++;
    }
  }
  return cnt;  
}

void SerialCommand::poolStream()
{
  if(IOstream->available())
  {
    receiveData((uint8_t) IOstream->read());
  }
}

