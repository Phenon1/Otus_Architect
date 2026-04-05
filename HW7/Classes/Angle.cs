using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ModelsProj.Classes
{
    public class Angle
    {
        const double _degreesPerSector = 9.0;
        const byte _sectorCount = 40;
        public sbyte sector;
        private static readonly double _radPerSector = (_degreesPerSector * Math.PI) / 180.0;


        public Angle(sbyte sector)
        {
            sbyte normalized = Convert.ToSByte(sector % _sectorCount);
            if(normalized < 0)
                normalized = Convert.ToSByte(_sectorCount + normalized);

            this.sector = normalized;
        }

        public double getAngleDegree()
        {
            return _degreesPerSector * sector;
        }

        public double getAngleRadian()
        {
            return _radPerSector * sector;
        }

        public sbyte getSector()
        {
            return sector;
        }

        public static Angle RotateTo(Angle currAngle, Angle rotateAngle)
        {
            return new Angle((sbyte)(currAngle.getSector() + rotateAngle.getSector()));
        }

    }
}
