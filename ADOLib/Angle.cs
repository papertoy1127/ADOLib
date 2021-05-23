﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ADOLib
{
    public class Angle
    {
        public static Dictionary<char, Angle> CharToAngle = new Dictionary<char, Angle>
        {
            {'R', new Angle(0, 'R', AngleType.Normal)},
            {'p', new Angle(15, 'p', AngleType.Normal)},
            {'J', new Angle(30, 'J', AngleType.Normal)},
            {'E', new Angle(45, 'E', AngleType.Normal)},
            {'T', new Angle(60, 'T', AngleType.Normal)},
            {'o', new Angle(75, 'o', AngleType.Normal)},
            {'U', new Angle(90, 'U', AngleType.Normal)},
            {'q', new Angle(105, 'q', AngleType.Normal)},
            {'G', new Angle(120, 'G', AngleType.Normal)},
            {'Q', new Angle(135, 'Q', AngleType.Normal)},
            {'H', new Angle(150, 'H', AngleType.Normal)},
            {'W', new Angle(165, 'W', AngleType.Normal)},
            {'L', new Angle(180, 'L', AngleType.Normal)},
            {'x', new Angle(195, 'x', AngleType.Normal)},
            {'N', new Angle(210, 'N', AngleType.Normal)},
            {'Z', new Angle(225, 'Z', AngleType.Normal)},
            {'F', new Angle(240, 'F', AngleType.Normal)},
            {'V', new Angle(255, 'V', AngleType.Normal)},
            {'D', new Angle(270, 'D', AngleType.Normal)},
            {'Y', new Angle(285, 'Y', AngleType.Normal)},
            {'B', new Angle(300, 'B', AngleType.Normal)},
            {'C', new Angle(315, 'C', AngleType.Normal)},
            {'M', new Angle(330, 'M', AngleType.Normal)},
            {'A', new Angle(345, 'A', AngleType.Normal)},
            {'!', new Angle(0, '!', AngleType.Midspin)},
            {'5', new Angle(108, '5', AngleType.CW)},
            {'6', new Angle(108, '6', AngleType.CCW)},
            {'7', new Angle(900M/7M, '7', AngleType.CW)},
            {'8', new Angle(900M/7M, '8', AngleType.CCW)},
            {'9', new Angle(210, '9', AngleType.CCW)}
        };
        public enum AngleType
        {
            Normal,
            CW,
            CCW,
            Midspin
        }
        public readonly decimal angle;
        public readonly AngleType angleType;
        public readonly char angleChar;

        private Angle(decimal angle, char angleChar, AngleType angleType)
        {
            this.angle = angle;
            this.angleChar = angleChar;
            this.angleType = angleType;
        }

        public static Angle GetAngle(char angleChar)
        {
            if (CharToAngle.ContainsKey(angleChar)) return CharToAngle[angleChar];
            return null;
        }

        public static Angle GetAngle(string angleName)
        {
            if (angleName.StartsWith("Angle")) return CharToAngle[(char) typeof(Angle).GetField(angleName).GetValue(null)];
                return null;
        }

        public const char Angle0 = 'R';
        public const char Angle15 = 'p';
        public const char Angle30 = 'J';
        public const char Angle45 = 'E';
        public const char Angle60 = 'T';
        public const char Angle75 = 'o';
        public const char Angle90 = 'U';
        public const char Angle105 = 'q';
        public const char Angle120 = 'G';
        public const char Angle135 = 'Q';
        public const char Angle150 = 'H';
        public const char Angle165 = 'W';
        public const char Angle180 = 'L';
        public const char Angle195 = 'x';
        public const char Angle210 = 'N';
        public const char Angle225 = 'Z';
        public const char Angle240 = 'F';
        public const char Angle255 = 'V';
        public const char Angle270 = 'D';
        public const char Angle285 = 'Y';
        public const char Angle300 = 'B';
        public const char Angle315 = 'C';
        public const char Angle330 = 'M';
        public const char Angle345 = 'A';
        public const char AngleMidspin = '!';
        public const char Angle108CW = '5';
        public const char Angle108CCW = '6';
        public const char Angle128CW = '7';
        public const char Angle128CCW = '8';
        public const char Angle210CW = '9';
        
        public static char Rotate(char Angle, int rotate)
        {
            rotate += 360; rotate %= 360;
            Angle angle = GetAngle(Angle);
            if (angle == null) return Angle;
            if (angle.angleType != AngleType.Normal) return Angle;
            decimal angleValue = angle.angle;
            angleValue += rotate;
            if (angleValue % 15 != 0) return Angle;
            angleValue = (angleValue + 360) % 360;
            return GetAngle($"Angle{angleValue}").angleChar;
        }

        public static string Rotate(string Angles, int rotate)
        {
            List<Angle> angles = new List<Angle>();
            StringBuilder angleString = new StringBuilder();
            foreach (var angle in Angles.ToCharArray())
            {
                angleString.Append(Rotate(angle, rotate));
            }

            return angleString.ToString();
        }
    }
}