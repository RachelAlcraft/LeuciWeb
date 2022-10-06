#pragma once
/*
* Class to support crystallography learning
*/

#include <string>
#include <vector>

using namespace std;

class VectorThree
{
public:
    double A;
    double B;
    double C;
    bool Valid;
    double getByIndex(int idx);
    void putByIndex(int idx, double val);
    VectorThree();
    VectorThree(bool isValid);
    VectorThree(double a, double b, double c);
    double distance(VectorThree ABC);
    double getMagnitude();
    double getDotProduct(VectorThree vec);
    VectorThree operator + (VectorThree const& obj);
    VectorThree operator - (VectorThree const& obj);
    VectorThree operator / (double val);
    double getAngle(VectorThree vec);
    string getKey();
    vector<VectorThree> getArcPositions(VectorThree end, int count);
    VectorThree reverse();
};

