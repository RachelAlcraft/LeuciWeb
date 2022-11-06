﻿using LeuciShared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LeuciShared
{
    public class VectorThree
    {
        public double A;
        public double B;
        public double C;
        public bool Valid;
        public VectorThree()
        {
            A = 0;
            B = 0;
            C = 0;
            Valid = true;
        }
        public VectorThree(bool isValid)
        {
            A = 0;
            B = 0;
            C = 0;
            Valid = isValid;
        }
        public VectorThree(double a, double b, double c)
        {
            A = a;
            B = b;
            C = c;
            Valid = true;
        }
        public double getByIndex(int idx)
        {
            if (idx == 0)
                return A;
            else if (idx == 1)
                return B;
            else // (idx == 0)
                return C;
        }
        public void putByIndex(int idx, double val)
        {
            if (idx == 0)
                A = val;
            else if (idx == 1)
                B = val;
            else // (idx == 0)
                C = val;
        }
        public double distance(VectorThree ABC)
        {
            double dis = (A - ABC.A) * (A - ABC.A) + (B - ABC.B) * (B - ABC.B) + (C - ABC.C) * (C - ABC.C);
            return Math.Sqrt(dis);
        }
        public double getMagnitude()
        {
            double mag = (A * A) + (B * B) + (C * C);
            return Math.Sqrt(mag);
        }
        public static VectorThree operator+ (VectorThree p, VectorThree q)
        {
            VectorThree r = new VectorThree();
            r.A = p.A + q.A;
            r.B = p.B + q.B;
            r.C = p.C + q.C;
            return r;
        }
        public static VectorThree operator- (VectorThree p, VectorThree q)
        {
            VectorThree r = new VectorThree();
            r.A = p.A - q.A;
            r.B = p.B - q.B;
            r.C = p.C - q.C;
            return r;
        }
        public static VectorThree operator/ (VectorThree p,double val)
        {
            VectorThree r = new VectorThree();
            r.A = p.A / val;
            r.B = p.B / val;
            r.C = p.C / val;
            return r;
        }

        public static VectorThree operator *(VectorThree p, double val)
        {
            VectorThree r = new VectorThree();
            r.A = p.A * val;
            r.B = p.B * val;
            r.C = p.C * val;
            return r;
        }
        public double getAngle(VectorThree vec)
        {
            VectorThree BA = new VectorThree(0 - A, 0 - B, 0 - C);
            VectorThree BC = new VectorThree(0 - vec.A, 0 - vec.B, 0 - vec.C);
            double dot = BA.getDotProduct(BC);
            double magBA = BA.getMagnitude();
            double magBC = BC.getMagnitude();
            double cosTheta = dot / (magBA * magBC);
            double theta = Math.Acos(cosTheta);
            return theta; //in radians
        }
        public double getDotProduct(VectorThree vec)
        {
            double px = A * vec.A;
            double py = B * vec.B;
            double pz = C * vec.C;
            return px + py + pz;
        }
        public string getKey()
        {
            return "(" + Convert.ToString(A)+","+ Convert.ToString(B) + "," + Convert.ToString(C) + ")";
        }
        public List<VectorThree> getArcPositions(VectorThree end, int count)
        {
            //First version is linear
            double diffX = A - end.A;
            double diffY = B - end.B;
            double diffZ = C - end.C;

            double gapX = 0;
            double gapY = 0;
            double gapZ = 0;
            if (count > 1)
            {
                gapX = diffX / (count - 1);
                gapY = diffY / (count - 1);
                gapZ = diffZ / (count - 1);
            }

            List<VectorThree> positions = new List<VectorThree>();
            for (int i = 0; i < count; ++i)
            {
                positions.Add(new VectorThree(A - i * gapX, B - i * gapY, C - i * gapZ));
            }
            return positions;

        }
        public VectorThree reverse()
        {
            return new VectorThree(C, B, A);
        }

        public VectorThree getPointPosition(double gap,double width)
        {
            VectorThree PP = new VectorThree(A, B, C);
            PP.A = PP.A / gap;
            PP.B = PP.B / gap;
            PP.C = PP.C / gap;
            int adj = (int)Math.Floor((width / (2 * gap)));
            int num = (int)Math.Floor((width / (2 * gap))) * 2 + 1;
            PP.A += (int)adj;// + 1;
            PP.B += (int)adj;
            //B.z += adj;

            //adjust in the x direction
            //PP.A = num - PP.A;

            return new VectorThree(PP.B,PP.A,PP.C);
        }
    }
}
