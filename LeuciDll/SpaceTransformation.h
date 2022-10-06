#pragma once
/************************************************************************
* RSA 10.9.21
************************************************************************/

#include <string>
#include <vector>

#include "VectorThree.h"

using namespace std;


class SpaceTransformation
{
private:
    VectorThree Central;
    VectorThree Linear;
    VectorThree Planar;
    //There are a set of transformations that need to be done in 1 order or the other to convert to the origin and then away.
    VectorThree _1_translation;
    double _2_rotationXY;
    double _3_rotationXZ;
    double _4_rotationYZ;
    //orthogonal unit axes
public:
    VectorThree xAxis;
    VectorThree yAxis;
    VectorThree zAxis;
    VectorThree xOrthog;
    VectorThree yOrthog;
    VectorThree zOrthog;
    VectorThree centre;
public:
    SpaceTransformation(VectorThree central, VectorThree linear, VectorThree planar);
    VectorThree applyTransformation(VectorThree point);
    VectorThree reverseTransformation(VectorThree point);
    double getRotationAngle(double x, double y);
    int getQuadrant(double x, double y);
    VectorThree rotate(double x, double y, double angle);
    VectorThree rotateNinety(double x_now, double y_now);
    VectorThree rotateQuadrant(double x, double y, double angle);

};