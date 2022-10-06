
/************************************************************************
* RSA 28.11.21
************************************************************************/
#include "pch.h" // use stdafx.h in Visual Studio 2017 and earlier
#include <iostream>
#include <fstream>
#include <vector>
#include <algorithm>
#include <sstream>
#include <cmath>
#include <cstring> // memcpy
#include "VectorThree.h"
#include <iomanip>

#include "Ccp4File.h"
#include "Helper.h"


using namespace std;

typedef unsigned char uchar;

Ccp4File::Ccp4File(vector<double> matrix)
{
    Matrix = matrix;
}

void Ccp4File::setWords(
    int w01_NC,
    int w02_NR,
    int w03_NS,
    int w04_Mode,
    int w05_NCSTART,
    int w06_NRSTART,
    int w07_NSSTART,
    int w08_NX,
    int w09_NY,
    int w10_NZ,
    double w11_CELLA_X,
    double w12_CELLA_Y,
    double w13_CELLA_Z,
    double w14_CELLB_X,
    double w15_CELLB_Y,
    double w16_CELLB_Z,
    int w17_MAPC,
    int w18_MAPR,
    int w19_MAPS)
{
    W01_NC = w01_NC;
    W02_NR = w02_NR;
    W03_NS = w03_NS;
    W04_Mode = w04_Mode;
    W05_NCSTART = w05_NCSTART;
    W06_NRSTART = w06_NRSTART;
    W07_NSSTART = w07_NSSTART;
    W08_NX = w08_NX;
    W09_NY = w09_NY;
    W10_NZ = w10_NZ;
    W11_CELLA_X = w11_CELLA_X;
    W12_CELLA_Y = w12_CELLA_Y;
    W13_CELLA_Z = w13_CELLA_Z;
    W14_CELLB_X = w14_CELLB_X;
    W15_CELLB_Y = w15_CELLB_Y;
    W16_CELLB_Z = w16_CELLB_Z;
    W17_MAPC = w17_MAPC;
    W18_MAPR = w18_MAPR;
    W19_MAPS = w19_MAPS;
    //W17_MAPC -= 1;
    //W18_MAPR -= 1;
    //W19_MAPS -= 1;

    loadInfo();

}

void Ccp4File::loadInfo()
{
    PI = 3.14159265;
            
    int len = W01_NC * W02_NR * W03_NS;    
    calculateOrthoMat(W11_CELLA_X, W12_CELLA_Y, W13_CELLA_Z, W14_CELLB_X, W15_CELLB_Y, W16_CELLB_Z);
    calculateOrigin(W05_NCSTART, W06_NRSTART, W07_NSSTART, W17_MAPC, W18_MAPR, W19_MAPS);

    _map2xyz.push_back(0);
    _map2xyz.push_back(0);
    _map2xyz.push_back(0);
    _map2xyz[W17_MAPC] = 0;
    _map2xyz[W18_MAPR] = 1;
    _map2xyz[W19_MAPS] = 2;

    _map2crs.push_back(0);
    _map2crs.push_back(0);
    _map2crs.push_back(0);
    _map2crs[0] = W17_MAPC;
    _map2crs[1] = W18_MAPR;
    _map2crs[2] = W19_MAPS;

    _cellDims.push_back(0.0);
    _cellDims.push_back(0.0);
    _cellDims.push_back(0.0);
    _cellDims[0] = W11_CELLA_X;
    _cellDims[1] = W12_CELLA_Y;
    _cellDims[2] = W13_CELLA_Z;

    _axisSampling.push_back(0);
    _axisSampling.push_back(0);
    _axisSampling.push_back(0);
    _axisSampling[0] = W08_NX;
    _axisSampling[1] = W09_NY;
    _axisSampling[2] = W10_NZ;

    _crsStart.push_back(0);
    _crsStart.push_back(0);
    _crsStart.push_back(0);
    _crsStart[0] = W05_NCSTART;
    _crsStart[1] = W06_NRSTART;
    _crsStart[2] = W07_NSSTART;

    _dimOrder.push_back(0);
    _dimOrder.push_back(0);
    _dimOrder.push_back(0);
    _dimOrder[0] = W01_NC;
    _dimOrder[1] = W02_NR;
    _dimOrder[2] = W03_NS;


}

VectorThree Ccp4File::getCRS(int position)
{
    int sliceSize = W01_NC * W02_NR;
    int i = position / sliceSize;
    int remainder = position % sliceSize;
    int j = remainder / W01_NC;
    int k = remainder % W01_NC;
    VectorThree CRS(i, j, k);
    return CRS;
}

void Ccp4File::calculateOrthoMat(float w11_CELLA_X, float w12_CELLA_Y, float w13_CELLA_Z, float w14_CELLB_X, float w15_CELLB_Y, float w16_CELLB_Z)
{
    // Cell angles is w14_CELLB_X, w15_CELLB_Y, w16_CELLB_Z
    // Cell lengths is w11_CELLA_X , w12_CELLA_Y , w13_CELLA_Z 
    double alpha = PI / 180 * w14_CELLB_X;
    double beta = PI / 180 * w15_CELLB_Y;
    double gamma = PI / 180 * w16_CELLB_Z;
    double temp = sqrt(1 - pow(cos(alpha), 2) - pow(cos(beta), 2) - pow(cos(gamma), 2) + 2 * cos(alpha) * cos(beta) * cos(gamma));

    double v00 = w11_CELLA_X;
    double v01 = w12_CELLA_Y * cos(gamma);
    double v02 = w13_CELLA_Z * cos(beta);
    double v10 = 0;
    double v11 = w12_CELLA_Y * sin(gamma);
    double v12 = w13_CELLA_Z * (cos(alpha) - cos(beta) * cos(gamma)) / sin(gamma);
    double v20 = 0;
    double v21 = 0;
    double v22 = w13_CELLA_Z * temp / sin(gamma);

    _orthoMat.putValue(w11_CELLA_X, 0, 0);
    _orthoMat.putValue(w12_CELLA_Y * cos(gamma), 0, 1);
    _orthoMat.putValue(w13_CELLA_Z * cos(beta), 0, 2);
    _orthoMat.putValue(0, 1, 0);
    _orthoMat.putValue(w12_CELLA_Y * sin(gamma), 1, 1);
    _orthoMat.putValue(w13_CELLA_Z * (cos(alpha) - cos(beta) * cos(gamma)) / sin(gamma), 1, 2);
    _orthoMat.putValue(0, 2, 0);
    _orthoMat.putValue(0, 2, 1);
    _orthoMat.putValue(w13_CELLA_Z * temp / sin(gamma), 2, 2);
    _deOrthoMat = _orthoMat.getInverse();
}

void Ccp4File::calculateOrigin(int w05_NXSTART, int w06_NYSTART, int w07_NZSTART, int w17_MAPC, int w18_MAPR, int w19_MAPS)
{
    /****************************
    * These comments are from my C# version and I have no idea currently what they mean (RSA 6/9/21)
    * ******************************
     *TODO I am ignoring the possibility of passing in the origin for nowand using the dot product calc for non orthoganality.
     *The origin is perhaps used for cryoEM only and requires orthoganility
     *CRSSTART is w05_NXSTART, w06_NYSTART, w07_NZSTART
     *Cell dims w08_MX, w09_MY, w10_MZ;
     *Map of indices from crs to xyz is w17_MAPC, w18_MAPR, w19_MAPS
     */

    VectorThree oro;

    for (int i = 0; i < 3; ++i)
    {
        int startVal = 0;
        if (w17_MAPC == i)
            startVal = w05_NXSTART;
        else if (w18_MAPR == i)
            startVal = w06_NYSTART;
        else
            startVal = w07_NZSTART;

        oro.putByIndex(i, startVal);
    }
    oro.putByIndex(0, oro.getByIndex(0) / W08_NX);
    oro.putByIndex(1, oro.getByIndex(1) / W09_NY);
    oro.putByIndex(2, oro.getByIndex(2) / W10_NZ);
    _origin = _orthoMat.multiply(oro, true);
}

VectorThree Ccp4File::getXYZFromCRS(VectorThree vCRSIn)
{
    VectorThree vXYZ;

    //If the axes are all orthogonal            
    if (W14_CELLB_X == 90 && W15_CELLB_Y == 90 && W16_CELLB_Z == 90)
    {
        for (int i = 0; i < 3; ++i)
        {
            double startVal = vCRSIn.getByIndex(_map2xyz[i]);
            startVal *= _cellDims[i] / _axisSampling[i];
            startVal += _origin.getByIndex(i);
            vXYZ.putByIndex(i, startVal);
        }
    }
    else // they are not orthogonal
    {
        VectorThree vCRS;
        for (int i = 0; i < 3; ++i)
        {
            double startVal = 0;
            if (W17_MAPC == i)
                startVal = W05_NCSTART + vCRSIn.A;
            else if (W18_MAPR == i)
                startVal = W06_NRSTART + vCRSIn.B;
            else
                startVal = W07_NSSTART + vCRSIn.C;
            vCRS.putByIndex(i, startVal);
        }
        vCRS.putByIndex(0, vCRS.getByIndex(0) / W08_NX);
        vCRS.putByIndex(1, vCRS.getByIndex(1) / W09_NY);
        vCRS.putByIndex(2, vCRS.getByIndex(2) / W10_NZ);
        vXYZ = _orthoMat.multiply(vCRS, false);
    }
    return vXYZ;
}

VectorThree Ccp4File::getCRSFromXYZ(VectorThree XYZ)
{
    VectorThree vCRS;
    //If the axes are all orthogonal            
    if (W14_CELLB_X == 90 && W15_CELLB_Y == 90 && W16_CELLB_Z == 90)
    {
        for (int i = 0; i < 3; ++i)
        {
            double startVal = XYZ.getByIndex(i) - _origin.getByIndex(i);
            startVal /= _cellDims[i] / _axisSampling[i];
            //vCRS[i] = startVal;
            vCRS.putByIndex(i, startVal);
        }
    }
    else // they are not orthogonal
    {
        VectorThree vFraction = _deOrthoMat.multiply(XYZ, true);
        for (int i = 0; i < 3; ++i)
        {
            double val = vFraction.getByIndex(i) * _axisSampling[i] - _crsStart[_map2xyz[i]];
            vCRS.putByIndex(i, val);
        }
    }
    double c = vCRS.getByIndex(_map2crs[0]);
    double r = vCRS.getByIndex(_map2crs[1]);
    double s = vCRS.getByIndex(_map2crs[2]);

    VectorThree CRS;
    CRS.A = c;
    CRS.B = r;
    CRS.C = s;
    return CRS;

}