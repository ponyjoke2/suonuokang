using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartUSKit.SmartUSKit
{
    internal class EnhanceParameters
    {
        private byte ucFrequency;
        private byte ucBiopsyEnhance;
        private int nDynamicRange;
        private byte ucFrameMean;
        private byte ucCompound;
        private byte ucAcousticalPower;
        private byte ucHarmonic;
        private byte ucFocusPos;
        private byte ucFocusCnt;

        private bool updateCommand;
        public void setUpdateCommand(bool updateCommand)
        {
            this.updateCommand = updateCommand;
        }


        public void setUcFrequency(byte ucFrequency)
        {
            this.ucFrequency = ucFrequency;
            this.updateCommand = true;
        }

        public byte getUcFrequency()
        {
            return ucFrequency;
        }

        public void setUcBiopsyEnhance(byte ucBiopsyEnhance)
        {
            this.ucBiopsyEnhance = ucBiopsyEnhance;
            this.updateCommand = true;
        }

        public byte getUcBiopsyEnhance()
        {
            return ucBiopsyEnhance;
        }

        public void setnDynamicRange(int nDynamicRange)
        {
            this.nDynamicRange = nDynamicRange;
            this.updateCommand = true;
        }

        public int getnDynamicRange()
        {
            return nDynamicRange;
        }

        public void setUcFrameMean(byte ucFrameMean)
        {
            this.ucFrameMean = ucFrameMean;
            this.updateCommand = true;
        }

        public byte getUcFrameMean()
        {
            return ucFrameMean;
        }

        public void setUcCompound(byte ucCompound)
        {
            this.ucCompound = ucCompound;
            this.updateCommand = true;
        }

        public byte getUcCompound()
        {
            return ucCompound;
        }

        public void setUcAcousticalPower(byte ucAcousticalPower)
        {
            this.ucAcousticalPower = ucAcousticalPower;
            this.updateCommand = true;
        }

        public byte getUcAcousticalPower()
        {
            return ucAcousticalPower;
        }

        public void setUcHarmonic(byte ucHarmonic)
        {
            this.ucHarmonic = ucHarmonic;
            this.updateCommand = true;
        }

        public byte getUcHarmonic()
        {
            return ucHarmonic;
        }

        public void setUcFocusPos(byte ucFocusPos)
        {
            this.ucFocusPos = ucFocusPos;
            this.updateCommand = true;
        }

        public byte getUcFocusPos()
        {
            return ucFocusPos;
        }

        public void setUcFocusCnt(byte ucFocusCnt)
        {
            this.ucFocusCnt = ucFocusCnt;
            this.updateCommand = true;
        }

        public byte getUcFocusCnt()
        {
            return ucFocusCnt;
        }

        public byte[] makeCommand()
        {
            if (!this.updateCommand)
            {
                return null;
            }
            updateCommand = false;

            byte[] ctrlblock = new byte[16];
            ctrlblock[0] = (byte)0x5F;
            ctrlblock[1] = (byte)0xF5;

            ctrlblock[2] = 0;
            ctrlblock[3] = 0;

            ctrlblock[4] = ucFrequency;
            ctrlblock[5] = ucBiopsyEnhance;
            ctrlblock[6] = (byte)(nDynamicRange & 0xFF);
            ctrlblock[7] = ucFrameMean;
            ctrlblock[8] = ucCompound;
            ctrlblock[9] = ucAcousticalPower;
            ctrlblock[10] = ucHarmonic;
            ctrlblock[11] = ucFocusPos;
            ctrlblock[12] = ucFocusCnt;
            ctrlblock[13] = 0;
            ctrlblock[14] = 0;
            ctrlblock[15] = 0;
            int sum = 0;
            for (int i = 0; i <= 15; i++)
            {
                sum += (int)(ctrlblock[i] & 0xFF);
            }
            sum = (int)(sum & 0xFF);
            ctrlblock[15] = (byte)((0 - sum) & 0xFF);
            return ctrlblock;
        }
        
    }
}
