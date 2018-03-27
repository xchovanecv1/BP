#ifndef CRC_H
#define CRC_H

/**********************           CRC32 Hardware *************************************
 *            
 *            
 *            
 *            
 */

typedef struct crc_reg_map {
    __io uint32 DR;              /**< Data register */
    __io uint32 IDR;            /**< Independent data register */
    __io uint32 CR;             /**< Control register */
    uint32 reserved;
    __io uint32 INIT;           /**< Initial data register */
    __io uint32 POL;            /**< Polynomial register */
} crc_reg_map;

/*
 * Register map base pointers
 */

/** CRC register map base pointer */
#define CRC_BASE                     ((struct crc_reg_map*)0x40023000) //TODO

/*
 * Register bit definitions
 */

/* Data register */
#define CRC_DR_DATA           0xFFFFFFFF

/* Independent data register */
#define CRC_IDR_DATA          0xFF

/* Control register */
#define CRC_CR_REV_OUT_BIT    7
#define CRC_CR_REV_IN_SHIFT   5
#define CRC_CR_POLYSIZE_SHIFT 3
#define CRC_CR_RESET_BIT      0

#define CRC_CR_REV_OUT        (1U << CRC_CR_REV_OUT_BIT)
#define CRC_CR_REV_IN         (0x3 << CRC_CR_REV_IN_SHIFT)
#define CRC_CR_POLYSIZE       (0x3 << CRC_CR_POLYSIZE_SHIFT)
#define CRC_CR_RESET          (1U << CRC_CR_RESET_BIT)

/* Initial data register */
#define CRC_INIT_DATA         0xFFFFFFFF

/* Polynomial register */
#define CRC_POL_DATA          0xFFFFFFFF

#define crcReset() (CRC_BASE->CR |= CRC_CR_RESET)
//#define crcReset() (CRC->CR |= CRC_CR_RESET)
#define crcRead() (CRC_BASE->DR)
#define crcWrite(x) {CRC_BASE->DR = x;}
/*
uint32_t CRC32(uint8_t *pBuf, uint8_t nSize)
{
    uint32_t index = 0;
    crcReset();        //CRC 
    for(index = 0; index < nSize; index++)
    {
      CRC_BASE->DR = (uint32_t)pBuf[index];
    }
    return (CRC_BASE->DR);
}

*/

#endif
