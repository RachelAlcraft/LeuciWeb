


/*
* 3x3 Matrix class
*  0 1 2
*  3 4 5
*  6 7 8
*/
#include "pch.h" // use stdafx.h in Visual Studio 2017 and earlier
#include "MatrixThreeThree.h"

MatrixThreeThree::MatrixThreeThree()
{
    for (int i = 0; i < 9; ++i)
    {
        _matrix.push_back(0);
    }
}

MatrixThreeThree::MatrixThreeThree(vector<double> vals)
{
    for (int i = 0; i < 9; ++i)
    {
        _matrix.push_back(vals[i]);
    }
}

MatrixThreeThree MatrixThreeThree::getInverse()
{
    double detWhole = getDeterminant();
    vector<double> transp;
    transp.push_back(_matrix[0]);
    transp.push_back(_matrix[3]);
    transp.push_back(_matrix[6]);
    transp.push_back(_matrix[1]);
    transp.push_back(_matrix[4]);
    transp.push_back(_matrix[7]);
    transp.push_back(_matrix[2]);
    transp.push_back(_matrix[5]);
    transp.push_back(_matrix[8]);
    /*transp.push_back(_matrix[0]);
    transp.push_back(_matrix[1]);
    transp.push_back(_matrix[2]);
    transp.push_back(_matrix[3]);
    transp.push_back(_matrix[4]);
    transp.push_back(_matrix[5]);
    transp.push_back(_matrix[6]);
    transp.push_back(_matrix[7]);
    transp.push_back(_matrix[8]);*/

    MatrixThreeThree transpose(transp);
    MatrixThreeThree matinverse;
    MatrixThreeThree matinverseSwitch;

    int factor = 1;

    for (int i = 0; i < 3; ++i)
    {
        for (int j = 0; j < 3; ++j)
        {
            double thisValue = transpose.getValue(i, j);
            double detReduced = transpose.getInnerDeterminant(i, j);
            matinverse.putValue(detReduced * factor / detWhole, i, j);
            factor *= -1;
        }
    }
    //I think I have the rows and columns mixed up, so I am going to switch them round just in case    
    /*MatrixThreeThree transpose2;
    transpose2.putValue(matinverse.getValue(0,0),0,0);
    transpose2.putValue(matinverse.getValue(0,1),1,0);
    transpose2.putValue(matinverse.getValue(0,2),2,0);
    transpose2.putValue(matinverse.getValue(1,0),0,1);
    transpose2.putValue(matinverse.getValue(1,1),1,1);
    transpose2.putValue(matinverse.getValue(1,2),2,1);
    transpose2.putValue(matinverse.getValue(2,0),0,2);
    transpose2.putValue(matinverse.getValue(2,1),1,2);
    transpose2.putValue(matinverse.getValue(2,2),2,2);
    return transpose2;*/

    return matinverse;

}

double MatrixThreeThree::getDeterminant()
{
    int factor = -1;
    double det = 0;
    for (int i = 0; i < 3; ++i)
    {
        factor = factor * -1;
        double row_val = _matrix[3 * i];
        double newdet = getInnerDeterminant(i, 0);
        det = det + (factor * row_val * newdet);
    }
    return det;
}

double MatrixThreeThree::getInnerDeterminant(int col, int row)
{
    vector<double> smallMat;
    if (col == 0)
    {
        if (row != 0)
        {
            smallMat.push_back(_matrix[1]);
            smallMat.push_back(_matrix[2]);
        }
        if (row != 1)
        {
            smallMat.push_back(_matrix[4]);
            smallMat.push_back(_matrix[5]);
        }
        if (row != 2)
        {
            smallMat.push_back(_matrix[7]);
            smallMat.push_back(_matrix[8]);
        }
    }
    else if (col == 1)
    {
        if (row != 0)
        {
            smallMat.push_back(_matrix[0]);
            smallMat.push_back(_matrix[2]);
        }
        if (row != 1)
        {
            smallMat.push_back(_matrix[3]);
            smallMat.push_back(_matrix[5]);
        }
        if (row != 2)
        {
            smallMat.push_back(_matrix[6]);
            smallMat.push_back(_matrix[8]);
        }
    }
    else// (col == 1)
    {
        if (row != 0)
        {
            smallMat.push_back(_matrix[0]);
            smallMat.push_back(_matrix[1]);
        }
        if (row != 1)
        {
            smallMat.push_back(_matrix[3]);
            smallMat.push_back(_matrix[4]);
        }
        if (row != 2)
        {
            smallMat.push_back(_matrix[6]);
            smallMat.push_back(_matrix[7]);
        }
    }

    double n11 = smallMat[0];
    double n12 = smallMat[1];
    double n21 = smallMat[2];
    double n22 = smallMat[3];

    return n11 * n22 - n12 * n21;
}

double MatrixThreeThree::getValue(int row, int col)
{
    int pos = row * 3 + col;
    return _matrix[pos];
}

void MatrixThreeThree::putValue(double val, int row, int col)
{
    int pos = row * 3 + col;
    _matrix[pos] = val;
}

VectorThree MatrixThreeThree::multiply(VectorThree col, bool byRow)
{
    //So, this is by row not by column, or,,, anyway which is which...
    double col0 = col.A;
    double col1 = col.B;
    double col2 = col.C;

    VectorThree scaled;

    double s0 = col0 * _matrix[byRow ? 0 : 0];
    double s1 = col0 * _matrix[byRow ? 1 : 3];
    double s2 = col0 * _matrix[byRow ? 2 : 6];

    s0 += col1 * _matrix[byRow ? 3 : 1];
    s1 += col1 * _matrix[byRow ? 4 : 4];
    s2 += col1 * _matrix[byRow ? 5 : 7];

    s0 += col2 * _matrix[byRow ? 6 : 2];
    s1 += col2 * _matrix[byRow ? 7 : 5];
    s2 += col2 * _matrix[byRow ? 8 : 8];

    scaled.putByIndex(0, s0);
    scaled.putByIndex(1, s1);
    scaled.putByIndex(2, s2);

    return scaled;
}
