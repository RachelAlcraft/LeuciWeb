
#include "pch.h" // use stdafx.h in Visual Studio 2017 and earlier

#include <cmath>
#include <sstream>
#include <iomanip>

#include "VectorThree.h"

using namespace std;

VectorThree::VectorThree()
{
    A = 0;
    B = 0;
    C = 0;
    Valid = true;
}

VectorThree::VectorThree(bool isValid)
{
    A = 0;
    B = 0;
    C = 0;
    Valid = isValid;
}


VectorThree::VectorThree(double a, double b, double c)
{
    A = a;
    B = b;
    C = c;
    Valid = true;
}

double VectorThree::getByIndex(int idx)
{
    if (idx == 0)
        return A;
    else if (idx == 1)
        return B;
    else // (idx == 0)
        return C;
}

void VectorThree::putByIndex(int idx, double val)
{
    if (idx == 0)
        A = val;
    else if (idx == 1)
        B = val;
    else // (idx == 0)
        C = val;
}

double VectorThree::distance(VectorThree ABC)
{
    double dis = (A - ABC.A) * (A - ABC.A) + (B - ABC.B) * (B - ABC.B) + (C - ABC.C) * (C - ABC.C);
    return sqrt(dis);
}

double VectorThree::getMagnitude()
{
    double mag = (A * A) + (B * B) + (C * C);
    return sqrt(mag);
}
VectorThree VectorThree::operator+(VectorThree const& obj)
{
    A += obj.A;
    B += obj.B;
    C += obj.C;
    return VectorThree(A, B, C);
}
VectorThree VectorThree::operator-(VectorThree const& obj)
{
    A -= obj.A;
    B -= obj.B;
    C -= obj.C;
    return VectorThree(A, B, C);
}

VectorThree VectorThree::operator/(double val)
{
    A /= val;
    B /= val;
    C /= val;
    return VectorThree(A, B, C);
}
double VectorThree::getAngle(VectorThree vec)
{
    VectorThree BA(0 - A, 0 - B, 0 - C);
    VectorThree BC(0 - vec.A, 0 - vec.B, 0 - vec.C);
    double dot = BA.getDotProduct(BC);
    double magBA = BA.getMagnitude();
    double magBC = BC.getMagnitude();
    double cosTheta = dot / (magBA * magBC);
    double theta = acos(cosTheta);
    return theta; //in radians
}

double VectorThree::getDotProduct(VectorThree vec)
{
    double px = A * vec.A;
    double py = B * vec.B;
    double pz = C * vec.C;
    return px + py + pz;
}
string VectorThree::getKey()
{
    stringstream ss;
    ss << setprecision(2) << fixed << A << B << C;
    return ss.str();
}

vector<VectorThree> VectorThree::getArcPositions(VectorThree end, int count)
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

    vector<VectorThree> positions;
    for (unsigned int i = 0; i < count; ++i)
    {
        positions.push_back(VectorThree(A - i * gapX, B - i * gapY, C - i * gapZ));
    }
    return positions;

}
VectorThree VectorThree::reverse()
{
    return VectorThree(C, B, A);
}
