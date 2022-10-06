#include "pch.h"



#define _USE_MATH_DEFINES
#include <cmath>


#include "SpaceTransformation.h"

using namespace std;



SpaceTransformation::SpaceTransformation(VectorThree central, VectorThree linear, VectorThree planar)
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
    //vR = rotate(pla.B, pla.C, _4_rotationYZ);
    pla.B = vR.A;
    pla.C = vR.B;

    //We have the transformations, now set up the orthogonal axes
    //Centre (0,0,0) should go to the central point            
    centre = applyTransformation(VectorThree(0, 0, 0));
    xOrthog = applyTransformation(VectorThree(1, 0, 0));
    xAxis = xOrthog - centre;
    yOrthog = applyTransformation(VectorThree(0, 1, 0));
    yAxis = yOrthog - centre;
    zOrthog = applyTransformation(VectorThree(0, 0, 1));
    zAxis = zOrthog - centre;

    //If these are othogonal the dot products will be zero, this is a debugging check
    VectorThree ortho(1, 0, 0);
    double o0 = ortho.getDotProduct(VectorThree(0, 1, 0));
    double o1 = xAxis.getDotProduct(yAxis);
    double o2 = xAxis.getDotProduct(zAxis);
    double o3 = zAxis.getDotProduct(yAxis);

}

double SpaceTransformation::getRotationAngle(double x, double y)
{
    double theta = 0.0;
    int qStart = getQuadrant(x, y);
    VectorThree vA;
    VectorThree axis;

    double mag = sqrt(pow(x, 2) + pow(y, 2));
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


int SpaceTransformation::getQuadrant(double x, double y)
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

VectorThree SpaceTransformation::rotate(double x, double y, double angle)
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

VectorThree SpaceTransformation::rotateNinety(double x_now, double y_now)
{
    int q = getQuadrant(x_now, y_now);
    q -= 1;
    if (q == 0)
        q = 4;
    VectorThree nextQ(abs(y_now), abs(x_now), 0);
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


VectorThree SpaceTransformation::rotateQuadrant(double x, double y, double angle)
{
    if (abs(angle) > 0.001)
    {
        VectorThree v;
        //This assumes an angle that is positive and less or = than 90 that may turn only into the next quadrant.            
        //Choose quadrants
        int qStart = getQuadrant(x, y);
        double mag = sqrt(pow(x, 2) + pow(y, 2)); // the length of the vector
        if (mag > 0.0001)
        {
            double sinA = abs(y) / mag;
            double angleA = asin(sinA); //this is the angle made with the x-axis from the original vector
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

            double x2 = cos(angleB) * mag;
            double y2 = sin(angleB) * mag;

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
        return VectorThree(x, y, 0);
    }
}

VectorThree SpaceTransformation::applyTransformation(VectorThree point)
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

VectorThree SpaceTransformation::reverseTransformation(VectorThree point)
{
    VectorThree pointPrime(point.A, point.B, point.C);
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




