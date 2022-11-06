using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace LeuciShared
{
    public class SpaceTransformation
    {        
        private VectorThree Central;
        private VectorThree Linear;
        private VectorThree Planar;

        //There are a set of transformations that need to be done in 1 order or the other to convert to the origin and then away.
        private VectorThree _1_translation;
        private double _2_rotationXY;
        private double _3_rotationXZ;
        private double _4_rotationYZ;
        //orthogonal unit axes       
        public VectorThree xAxis;
        public VectorThree yAxis;
        public VectorThree zAxis;
        public VectorThree xOrthog;
        public VectorThree yOrthog;
        public VectorThree zOrthog;
        public VectorThree centre;
                
        private const double M_PI = 3.14159265358979323846;

        public SpaceTransformation(VectorThree central, VectorThree linear, VectorThree planar)
        {
            //This is the constructor to create a transformation that maps the 3 given points onto the origin and flat against the plane xz.
            Central = central;
            Linear = linear;
            Planar = planar;
            //First transformation is to the origin, a translation
            _1_translation = central;
            VectorThree lin = linear;
            VectorThree pla = planar;
            lin = lin - _1_translation;
            pla = pla - _1_translation;

            //Rotation vectors                    
            VectorThree vR;

            //Second transformation is to rotate the linear vector to make y=0                        
            _2_rotationXY = getRotationAngle(lin.A, lin.B);
            vR = rotate(lin.A, lin.B, _2_rotationXY);
            lin.A = vR.A;
            lin.B = vR.B;
            vR = rotate(pla.A, pla.B, _2_rotationXY);
            pla.A = vR.A;
            pla.B = vR.B;

            //Third transformation is to rotate the linear vector to make z=0            
            _3_rotationXZ = getRotationAngle(lin.A, lin.C);
            vR = rotate(lin.A, lin.C, _3_rotationXZ);
            lin.A = vR.A;
            lin.C = vR.B;
            vR = rotate(pla.A, pla.C, _3_rotationXZ);
            pla.A = vR.A;
            pla.C = vR.B;

            //Fourth transformation is to rotate the planar vector to make z=0    
            _4_rotationYZ = getRotationAngle(pla.B, pla.C);
            vR = rotate(pla.A, pla.C, _4_rotationYZ); //!!!!!! IS THERE A BUG HERE, should it be the below, which is anyway never used...            
            pla.B = vR.A;
            pla.C = vR.B;

            //We have the transformations, now set up the orthogonal axes
            //Centre (0,0,0) should go to the central point            
            centre = applyTransformation(new VectorThree(0, 0, 0));
            xOrthog = applyTransformation(new VectorThree(1, 0, 0));
            xAxis = xOrthog - centre;
            yOrthog = applyTransformation(new VectorThree(0, 1, 0));
            yAxis = yOrthog - centre;
            zOrthog = applyTransformation(new VectorThree(0, 0, 1));
            zAxis = zOrthog - centre;

            //If these are othogonal the dot products will be zero, this is a debugging check
            VectorThree ortho = new VectorThree(1, 0, 0);
            double o0 = ortho.getDotProduct(new VectorThree(0, 1, 0));
            double o1 = xAxis.getDotProduct(yAxis);
            double o2 = xAxis.getDotProduct(zAxis);
            double o3 = zAxis.getDotProduct(yAxis);

        }        
        private double getRotationAngle(double x, double y)
        {
            double theta = 0.0;
            int qStart = getQuadrant(x, y);
            VectorThree vA = new VectorThree();
            VectorThree axis = new VectorThree();

            double mag = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
            if (mag > 0.0001)
            {
                vA.A = x;
                vA.B = y;
                axis.A = mag;
                axis.B = 0;
                theta = vA.getAngle(axis);
                //We want to go claockwise to the positive x-axis and this is just the absolute difference, so:        
                if (qStart == 4 || qStart == 3)
                    theta = 2 * M_PI - theta;
                if (theta < 0)
                    theta = 2 * M_PI + theta;
            }
            return theta;
        }


        private int getQuadrant(double x, double y)
        {
            //Choose quadrants            
            int qStart = 1;
            //first if it is on an axis
            if (x == 0 && y > 0)
                qStart = 1;
            else if (x == 0 && y < 0)
                qStart = 4;
            else if (y == 0 && x > 0)
                qStart = 1;
            else if (y == 0 && x < 0)
                qStart = 2;
            else if (x < 0 && y < 0)
                qStart = 3;
            else if (x < 0 && y > 0)
                qStart = 2;
            else if (y < 0 && x > 0)
                qStart = 4;

            return qStart;
        }

        private VectorThree rotate(double x, double y, double angle)
        {

            double angle_left = angle;
            double x_now = x;
            double y_now = y;
            VectorThree pointPrime;

            while (angle_left > M_PI / 2)
            {
                //EdVector pointPrime2 = rotateQuadrant(x_now, y_now, Math.PI / 2);
                pointPrime = rotateNinety(x_now, y_now);
                x_now = pointPrime.A;
                y_now = pointPrime.B;
                angle_left -= M_PI / 2;
            }
            pointPrime = rotateQuadrant(x_now, y_now, angle_left);
            return pointPrime;
        }

        private VectorThree rotateNinety(double x_now, double y_now)
        {
            int q = getQuadrant(x_now, y_now);
            q -= 1;
            if (q == 0)
                q = 4;
            VectorThree nextQ = new VectorThree(Math.Abs(y_now), Math.Abs(x_now), 0);
            if (q == 2)
            {
                nextQ.A *= -1;
            }
            else if (q == 3)
            {
                nextQ.A *= -1;
                nextQ.B *= -1;
            }
            else if (q == 4)
            {
                nextQ.B *= -1;
            }
            return nextQ;
        }


        private VectorThree rotateQuadrant(double x, double y, double angle)
        {
            if (Math.Abs(angle) > 0.001)
            {
                VectorThree v = new VectorThree();
                //This assumes an angle that is positive and less or = than 90 that may turn only into the next quadrant.            
                //Choose quadrants
                int qStart = getQuadrant(x, y);
                double mag = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)); // the length of the vector
                if (mag > 0.0001)
                {
                    double sinA = Math.Abs(y) / mag;
                    double angleA = Math.Asin(sinA); //this is the angle made with the x-axis from the original vector
                    double angleB = angleA - angle; // this is the angle made with the x-axis with the rotated vector

                    int qEnd = qStart;
                    if (qStart == 1)
                    {
                        if (angle > angleA)
                        {
                            angleB = angle - angleA;
                            qEnd = 4;
                        }
                    }
                    else if (qStart == 2)
                    {
                        angleB = angle + angleA;
                        if (angleA + angle > M_PI / 2)
                        {
                            angleB = M_PI - (angleA + angle);
                            qEnd = 1;
                        }
                    }
                    else if (qStart == 3)
                    {
                        if (angle > angleA)
                        {
                            angleB = angle - angleA;
                            qEnd = 2;
                        }
                    }
                    else // must be q4
                    {
                        angleB = angle + angleA;
                        if (angleA + angle > M_PI / 2)
                        {
                            angleB = M_PI - (angle + angleA);
                            qEnd = 3;
                        }
                    }

                    double x2 = Math.Cos(angleB) * mag;
                    double y2 = Math.Sin(angleB) * mag;

                    if (qEnd == 2 || qEnd == 3)
                        x2 *= -1;
                    if (qEnd == 3 || qEnd == 4)
                        y2 *= -1;

                    v.A = x2;//Math.Round(x2, 8);
                    v.B = y2;//Math.Round(y2, 8);
                }
                return v;
            }
            else
            {
                return new VectorThree(x, y, 0);
            }
        }

        public VectorThree applyTransformation(VectorThree point)
        {            
            VectorThree pointPrime = point;
            VectorThree point2;
            
            double rotationYZ_4 = _4_rotationYZ;
            double rotationXZ_3 = _3_rotationXZ;
            double rotationXY_2 = _2_rotationXY;

            point2 = rotate(pointPrime.B, pointPrime.C, 2 * M_PI - rotationYZ_4);
            pointPrime.B = point2.A;
            pointPrime.C = point2.B;

            point2 = rotate(pointPrime.A, pointPrime.C, 2 * M_PI - rotationXZ_3);
            pointPrime.A = point2.A;
            pointPrime.C = point2.B;

            point2 = rotate(pointPrime.A, pointPrime.B, 2 * M_PI - rotationXY_2);
            pointPrime.A = point2.A;
            pointPrime.B = point2.B;

            pointPrime = pointPrime + _1_translation;
            
            return pointPrime;
        }

        public VectorThree extraNav(VectorThree point,string nav,double nav_mag)
        {            
            double angle = 2*Math.PI / 90;
            if (nav == "down")
            {
                point += (xAxis *= nav_mag);
            }
            else if (nav == "up")
            {
                point -= (xAxis *= nav_mag);
            }
            else if (nav == "left")
            {
                point += (yAxis *= nav_mag);
            }
            else if (nav == "right")
            {
                point -= (yAxis *= nav_mag);
            }
            else if (nav == "fwd")
            {
                point += (zAxis *= nav_mag);
            }
            else if (nav == "back")
            {
                point -= (zAxis *= nav_mag);
            }
            else if (nav == "tilt_left")
            {
                point = reverseTransformation(point);
                VectorThree ll =rotate(point.B, point.C, angle);
                point.B = ll.A;
                point.C = ll.B;
                point = applyTransformation(point);
            }
            else if (nav == "tilt_right")
            {
                point = reverseTransformation(point);
                VectorThree ll = rotate(point.B, point.C, -1* angle);
                point.B = ll.A;
                point.C = ll.B;
                point = applyTransformation(point);
            }
            else if (nav == "tilt_up")
            {
                point = reverseTransformation(point);
                VectorThree ll = rotate(point.A, point.C, angle);
                point.A = ll.A;
                point.C = ll.B;
                point = applyTransformation(point);
            }
            else if (nav == "tilt_down")
            {
                point = reverseTransformation(point);
                VectorThree ll = rotate(point.A, point.C, -1* angle);
                point.A = ll.A;
                point.C = ll.B;
                point = applyTransformation(point);
            }
            else if (nav == "clock")
            {
                point = reverseTransformation(point);
                VectorThree ll = rotate(point.A, point.B, angle);
                point.A = ll.A;
                point.B = ll.B;
                point = applyTransformation(point);
            }
            else if (nav == "anti")
            {
                point = reverseTransformation(point);
                VectorThree ll = rotate(point.A, point.B, -1 * angle);
                point.A = ll.A;
                point.B = ll.B;
                point = applyTransformation(point);
            }
            
            return point;                
        }

        public VectorThree reverseTransformation(VectorThree point)
        {
            VectorThree pointPrime = new VectorThree(point.A, point.B, point.C);
            VectorThree point2;

            double rotationYZ_4 = _4_rotationYZ;
            double rotationXZ_3 = _3_rotationXZ;
            double rotationXY_2 = _2_rotationXY;

            pointPrime = pointPrime - _1_translation;

            point2 = rotate(pointPrime.A, pointPrime.B, rotationXY_2);
            pointPrime.A = point2.A;
            pointPrime.B = point2.B;

            point2 = rotate(pointPrime.A, pointPrime.C, rotationXZ_3);
            pointPrime.A = point2.A;
            pointPrime.C = point2.B;

            point2 = rotate(pointPrime.B, pointPrime.C, rotationYZ_4);
            pointPrime.B = point2.A;
            pointPrime.C = point2.B;
            
            return pointPrime;
        }
    }
}
