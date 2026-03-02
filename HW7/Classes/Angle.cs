using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace HW7.Classes
{
    public class Angle
    {
        const double _degreesPerSector = 9.0;
        const byte _sectorCount = 40;
        public sbyte sector;


        public Angle(sbyte sector)
        {
            sbyte normalized = Convert.ToSByte(sector % _sectorCount);
            if(normalized < 0)
                normalized = Convert.ToSByte(_sectorCount + normalized);

            this.sector = normalized;
        }
        public Angle(double angle)
        {
            var normalized = angle % 360.0;
            if (normalized < 0) 
                normalized += 360.0;
            sector = Convert.ToSByte(Math.Truncate(normalized / _degreesPerSector));
        }
        public double getAngle()
        {
            return _degreesPerSector * sector;
        }

        public sbyte getSector()
        {
            return sector;
        }

        public static Angle RotateTo(Angle currAngle, Angle rotateAngle)
        {
            return new Angle(currAngle.getSector() + rotateAngle.getSector());
        }

    }
}
