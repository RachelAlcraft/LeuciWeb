#include "pch.h"
#include "Interpolator.h"
#include <cmath>
#include <algorithm>

// ****** ABSTRACT CLASS ****************************************
Interpolator::Interpolator(vector<double> matrix, int x, int y, int z)
{
    Matrix = matrix;
    XLen = x;
    YLen = y; 
    ZLen = z;
    h = 0.001;
}

void Interpolator::addMatrix(vector<double> matrix, int x, int y, int z)
{
    Matrix = matrix;
    XLen = x;
    YLen = y;
    ZLen = z;
    h = 0.001;
}

int Interpolator::getPosition(int x, int y, int z)
{
    int sliceSize = XLen * YLen;
    int pos = z * sliceSize;
    pos += XLen * y;
    pos += x;
    if (pos > 0 && pos < Matrix.size())
        return pos;
    else
        return 0;//what should this return? -1, throw an error? TODO
}

float Interpolator::getExactValue(int x, int y, int z)
{
    int sliceSize = XLen * YLen;
    int pos = z * sliceSize;
    pos += XLen * y;
    pos += x;
    if (pos > 0 && pos < Matrix.size())
        return Matrix[pos];
    else
        return 0;
}

vector<double> Interpolator::getStatsValues()
{// returns min, q1, median,q3,max
    vector<double> tmpmat = Matrix;
    std::sort(tmpmat.rbegin(), tmpmat.rend());
    double min = tmpmat[0];
    double max = tmpmat[tmpmat.size() - 1];
    double q1 = tmpmat[int(tmpmat.size() * 0.25)];
    double q2 = tmpmat[int(tmpmat.size() * 0.5)];
    double q3 = tmpmat[int(tmpmat.size() * 0.75)];
    vector<double> stats;
    stats.push_back(max);
    stats.push_back(q3);
    stats.push_back(q2);
    stats.push_back(q1);
    stats.push_back(min);
    return stats;
}

vector<int> Interpolator::getStatsPoses()
{// returns min, q1, median,q3,max
    vector<double> tmpmat = Matrix;
    std::sort(tmpmat.rbegin(), tmpmat.rend());
    int q1 = int(tmpmat.size() * 0.25);
    int q2 = int(tmpmat.size() * 0.5);
    int q3 = int(tmpmat.size() * 0.75);
    vector<int> stats;
    stats.push_back(tmpmat.size() - 1);
    stats.push_back(q3);
    stats.push_back(q2);
    stats.push_back(q1);
    stats.push_back(0);
    return stats;
}

double Interpolator::getRadiant(double x, double y, double z)
{
    double val = getValue(x, y, z);
    double dx = (getValue(x + h, y, z) - val) / h;
    double dy = (getValue(x, y + h, z) - val) / h;
    double dz = (getValue(x, y, z + h) - val) / h;
    double radiant = (abs(dx) + abs(dy) + abs(dz)) / 3;
    return radiant;
}

double Interpolator::getLaplacian(double x, double y, double z)
{
    double val = getValue(x, y, z);
    double xx = getDxDx(x, y, z, val);
    double yy = getDyDy(x, y, z, val);
    double zz = getDzDz(x, y, z, val);
    return xx + yy + zz;
}

double Interpolator::getDxDx(double x, double y, double z, double val)
{
    double va = getValue(x - h, y, z);
    double vb = getValue(x + h, y, z);
    double dd = (va + vb - 2 * val) / (h * h);
    return dd;
}
double Interpolator::getDyDy(double x, double y, double z, double val)
{
    double va = getValue(x, y - h, z);
    double vb = getValue(x, y + h, z);
    double dd = (va + vb - 2 * val) / (h * h);
    return dd;
}
double Interpolator::getDzDz(double x, double y, double z, double val)
{
    double va = getValue(x, y, z - h);
    double vb = getValue(x, y, z + h);
    double dd = (va + vb - 2 * val) / (h * h);
    return dd;
}

VectorThree Interpolator::getNearbyAtomPeak(VectorThree XYZ, bool density)
{
    return getNearestPeakRecursive(XYZ, XYZ, density, 0, 0.5, 10, true);//we can't go on forever but we don;t want to imagine somehting that might not be there
}
VectorThree Interpolator::getNearbyGridPeak(VectorThree CRS, bool density)
{
    return getNearestPeakRecursive(CRS, CRS, density, 0, 0.5, 8, false);//we know we have a peak, so optimise it to give us an answer
}
VectorThree Interpolator::getNearestPeakRecursive(VectorThree Orig, VectorThree CRS, bool density, int level, double width, int cap, bool invalidNonConvergence)
{
    //the boot out of recursion step
    if (level > cap)
    {
        if (width <= 0.1)//Success
            return CRS;
        if (!invalidNonConvergence)
            return CRS;
        else
            return VectorThree(false);

    }

    //otherwise we either shrink the box or move to the biggest nearby.
    double biggestDensity = getValue(CRS.C, CRS.B, CRS.A);
    double smallestLaplacian = getLaplacian(CRS.C, CRS.B, CRS.A);
    VectorThree biggestCRS = CRS;
    bool haveFound = false;

    for (int a = -1; a < 2; ++a)
        for (int b = -1; b < 2; ++b)
            for (int c = -1; c < 2; ++c)
            {
                double i = a * width;
                double j = b * width;
                double k = c * width;

                if (a != 0 && b != 0 && c != 0)
                {
                    VectorThree abc = VectorThree(CRS.A + i, CRS.B + j, CRS.C + k);
                    double interpDensity = getValue(abc.C, abc.B, abc.A);
                    double interpLaplacian = getLaplacian(abc.C, abc.B, abc.A);
                    if ((density && interpDensity > biggestDensity) || (!density && interpLaplacian < smallestLaplacian))
                    {
                        biggestCRS = abc;
                        biggestDensity = interpDensity;
                        haveFound = true;
                    }
                }
            }
    if (haveFound)
        return getNearestPeakRecursive(Orig, biggestCRS, density, ++level, width, cap, invalidNonConvergence);
    else
        return getNearestPeakRecursive(Orig, biggestCRS, density, ++level, width * 0.75, cap, invalidNonConvergence);
}

// ****** Nearest Neighbour Implementation ****************************************
Nearest::Nearest(vector<double> matrix, int x, int y, int z) :Interpolator(matrix, x, y, z)
{
}

double Nearest::getValue(double x, double y, double z)
{
    int i = int(round(x));
    int j = int(round(y));
    int k = int(round(z));
    return getExactValue(i, j, k);
}
// ****** Nearest Neighbour Implementation ****************************************

// ****** Thevenaz Spline Convolution Implementation ****************************************
// Th?venaz, Philippe, Thierry Blu, and Michael Unser. ?Image Interpolation and Resampling?, n.d., 39.
//   http://bigwww.epfl.ch/thevenaz/interpolation/
// *******************************************************************************
Thevenaz::Thevenaz(vector<double> matrix, int x, int y, int z) :Interpolator(matrix, x, y, z)
{
    TOLERANCE = 2.2204460492503131e-016; // smallest such that 1.0+DBL_EPSILON != 1.0
    _degree = 5;
    createCoefficients();

}

double Thevenaz::getValue(double x, double y, double z)
{
    int weight_length = _degree + 1;
    vector<int> xIndex;
    vector<int> yIndex;
    vector<int> zIndex;
    vector<double> xWeight;
    vector<double> yWeight;
    vector<double> zWeight;
    for (int i = 0; i < weight_length; ++i)
    {
        xIndex.push_back(0);
        yIndex.push_back(0);
        zIndex.push_back(0);
        xWeight.push_back(0);
        yWeight.push_back(0);
        zWeight.push_back(0);
    }


    //Compte the interpolation indices
    int i = int(floor(x) - _degree / 2);
    int j = int(floor(y) - _degree / 2);
    int k = int(floor(z) - _degree / 2);

    for (int l = 0; l <= _degree; ++l)
    {
        xIndex[l] = i++; //if 71.1 passed in, for linear, we would want 71 and 72, 
        yIndex[l] = j++;
        zIndex[l] = k++;
    }

    /* compute the interpolation weights */

    if (_degree == 9)
    {
        xWeight = applyValue9(x, xIndex, weight_length);
        yWeight = applyValue9(y, yIndex, weight_length);
        zWeight = applyValue9(z, zIndex, weight_length);
    }
    else if (_degree == 7)
    {
        xWeight = applyValue7(x, xIndex, weight_length);
        yWeight = applyValue7(y, yIndex, weight_length);
        zWeight = applyValue7(z, zIndex, weight_length);
    }
    else if (_degree == 5)
    {
        xWeight = applyValue5(x, xIndex, weight_length);
        yWeight = applyValue5(y, yIndex, weight_length);
        zWeight = applyValue5(z, zIndex, weight_length);
    }
    else
    {
        xWeight = applyValue3(x, xIndex, weight_length);
        yWeight = applyValue3(y, yIndex, weight_length);
        zWeight = applyValue3(z, zIndex, weight_length);
    }

    //applying the mirror boundary condition becaue I am only interpolating within values??
    int Width2 = 2 * XLen - 2;
    int Height2 = 2 * YLen - 2;
    int Depth2 = 2 * ZLen - 2;
    for (k = 0; k <= _degree; k++)
    {
        xIndex[k] = (XLen == 1) ? (0) :
            ((xIndex[k] < 0) ?
                (-xIndex[k] - Width2 * ((-xIndex[k]) / Width2)) :
                (xIndex[k] - Width2 * (xIndex[k] / Width2)));
        if (XLen <= xIndex[k])
        {
            xIndex[k] = Width2 - xIndex[k];
        }

        yIndex[k] = (YLen == 1) ? (0) :
            ((yIndex[k] < 0) ?
                (-yIndex[k] - Height2 * ((-yIndex[k]) / Height2)) :
                (yIndex[k] - Height2 * (yIndex[k] / Height2)));
        if (YLen <= yIndex[k])
        {
            yIndex[k] = Height2 - yIndex[k];
        }

        zIndex[k] = (ZLen == 1) ? (0) :
            ((zIndex[k] < 0) ?
                (-zIndex[k] - Depth2 * ((-zIndex[k]) / Depth2)) :
                (zIndex[k] - Depth2 * (zIndex[k] / Depth2)));
        if (ZLen <= zIndex[k])
        {
            zIndex[k] = Depth2 - zIndex[k];
        }
    }

    //Perform interolation
    /* perform interpolation */
    int splineDegree = _degree;
    double w3 = 0.0;
    for (k = 0; k <= splineDegree; k++)
    {
        double w2 = 0.0;
        for (j = 0; j <= splineDegree; j++)
        {
            double w1 = 0.0;
            for (i = 0; i <= splineDegree; i++)
            {
                w1 += xWeight[i] * _coefficients[getPosition(xIndex[i], yIndex[j], zIndex[k])];
            }
            w2 += yWeight[j] * w1;
        }
        w3 += zWeight[k] * w2;
    }
    return w3;
}

void Thevenaz::createCoefficients()
{
    for (int i = 0; i < XLen * YLen * ZLen; ++i)
        _coefficients.push_back(Matrix[i]);

    vector<double> pole = getPole(_degree);
    int numPoles = (int)pole.size();

    //Convert the samples to interpolation coefficients
    //X-wise
    for (int y = 0; y < YLen; ++y)
    {
        for (int z = 0; z < ZLen; ++z)
        {
            vector<double> row = getRow3d(y, z, XLen);
            vector<double> line = convertToInterpolationCoefficients(pole, numPoles, XLen, row);
            putRow3d(y, z, line, XLen);
        }
    }
    //Y-wise
    for (int x = 0; x < XLen; ++x)
    {
        for (int z = 0; z < ZLen; ++z)
        {
            vector<double> row = getColumn3d(x, z, YLen);
            vector<double> line = convertToInterpolationCoefficients(pole, numPoles, YLen, row);
            putColumn3d(x, z, line, YLen);
        }
    }

    //Z-wise
    for (int x = 0; x < XLen; ++x)
    {
        for (int y = 0; y < YLen; ++y)
        {
            vector<double> row = getHole3d(x, y, ZLen);
            vector<double> line = convertToInterpolationCoefficients(pole, numPoles, ZLen, row);
            putHole3d(x, y, line, ZLen);
        }
    }
}

vector<double> Thevenaz::getRow3d(int y, int z, int length)
{
    vector<double> row;
    for (int x = 0; x < length; ++x)
        row.push_back(_coefficients[getPosition(x, y, z)]);
    return row;
}
void Thevenaz::putRow3d(int y, int z, vector<double> row, int length)
{
    for (int x = 0; x < length; ++x)
        _coefficients[getPosition(x, y, z)] = row[x];
}
vector<double> Thevenaz::getColumn3d(int x, int z, int length)
{
    vector<double> col;
    for (int y = 0; y < length; ++y)
        col.push_back(_coefficients[getPosition(x, y, z)]);
    return col;
}
void Thevenaz::putColumn3d(int x, int z, vector<double> col, int length)
{
    for (int y = 0; y < length; ++y)
        _coefficients[getPosition(x, y, z)] = col[y];
}
vector<double> Thevenaz::getHole3d(int x, int y, int length)
{
    vector<double> bore;
    for (int z = 0; z < length; ++z)
        bore.push_back(_coefficients[getPosition(x, y, z)]);
    return bore;
}
void Thevenaz::putHole3d(int x, int y, vector<double> bore, int length)
{
    for (int z = 0; z < length; ++z)
        _coefficients[getPosition(x, y, z)] = bore[z];
}

vector<double> Thevenaz::getPole(int degree)
{
    //Recover the poles from a lookup table #currently only 3 degree, will I want to calculate all the possibilities at the beginnning, 3,5,7,9?
    vector<double> pole;
    if (degree == 9)
    {
        pole.push_back(-0.60799738916862577900772082395428976943963471853991);
        pole.push_back(-0.20175052019315323879606468505597043468089886575747);
        pole.push_back(-0.043222608540481752133321142979429688265852380231497);
        pole.push_back(-0.0021213069031808184203048965578486234220548560988624);
    }
    else if (degree == 7)
    {
        pole.push_back(-0.53528043079643816554240378168164607183392315234269);
        pole.push_back(-0.12255461519232669051527226435935734360548654942730);
        pole.push_back(-0.0091486948096082769285930216516478534156925639545994);
    }
    else if (degree == 5)
    {
        pole.push_back(sqrt(135.0 / 2.0 - sqrt(17745.0 / 4.0)) + sqrt(105.0 / 4.0) - 13.0 / 2.0);
        pole.push_back(sqrt(135.0 / 2.0 + sqrt(17745.0 / 4.0)) - sqrt(105.0 / 4.0) - 13.0 / 2.0);
    }
    else//then it is 3
    {
        pole.push_back(sqrt(3.0) - 2.0);
    }
    return pole;
}

vector<double> Thevenaz::convertToInterpolationCoefficients(vector<double> pole, int numPoles, int width, vector<double> row)
{
    /* special case required by mirror boundaries */
    if (width == 1)
        return row; ;

    double lambda = 1;
    long n = 0;
    long k = 0;
    //Compute the overall gain
    for (k = 0; k < numPoles; k++)
    {
        lambda = lambda * (1 - pole[k]) * (1 - 1 / pole[k]);
    }
    //Apply the gain
    for (n = 0; n < width; n++)
    {
        row[n] *= lambda;
    }
    //loop over the poles            
    for (k = 0; k < numPoles; k++)
    {
        /* causal initialization */
        row[0] = InitialCausalCoefficient(row, width, pole[k]);
        /* causal recursion */
        for (n = 1; n < width; n++)
        {
            row[n] += (double)pole[k] * row[n - 1.0];
        }
        /* anticausal initialization */
        row[width - 1.0] = InitialAntiCausalCoefficient(row, width, pole[k]);
        /* anticausal recursion */
        for (n = width - 2; 0 <= n; n--)
        {
            row[n] = pole[k] * (row[n + 1.0] - row[n]);
        }
    }
    return row;
}

double Thevenaz::InitialCausalCoefficient(vector<double> c, long dataLength, double pole)

{ /* begin InitialCausalCoefficient */

    double Sum, zn, z2n, iz;
    long n, Horizon;

    /* this initialization corresponds to mirror boundaries */
    Horizon = dataLength;
    if (TOLERANCE > 0.0)
    {
        Horizon = (long)ceil(log(TOLERANCE) / log(abs(pole)));
    }
    if (Horizon < dataLength)
    {
        /* accelerated loop */
        zn = pole;
        Sum = c[0];
        for (n = 1L; n < Horizon; n++)
        {
            Sum += zn * c[n];
            zn *= pole;
        }
        return (Sum);
    }
    else
    {
        /* full loop */
        zn = pole;
        iz = 1.0 / pole;
        z2n = pow(pole, (double)(dataLength - 1.0));
        Sum = c[0] + z2n * (double)c[dataLength - 1.0];
        z2n *= z2n * iz; //is this a mistake, should it be just *=??? Checked it is how it is in their code. NO TRIED IT.                
        for (n = 1L; n <= dataLength - 2L; n++)
        {
            Sum += (zn + z2n) * c[n];
            zn *= pole;
            //z2n *= z2n * iz;
            z2n *= iz;
        }
        return (Sum / (1.0 - zn * zn));
    }
}
double Thevenaz::InitialAntiCausalCoefficient(vector<double> c, long dataLength, double pole)
{
    /* this initialization corresponds to mirror boundaries */
    if (dataLength < 2)
        return 0;
    else
        return ((pole / (pole * pole - 1.0)) * (pole * c[dataLength - 2.0] + c[dataLength - 1.0]));
}
vector<double> Thevenaz::applyValue3(double val, vector<int> idc, int weight_length)
{
    vector<double> ws;
    for (int i = 0; i < weight_length; ++i)
        ws.push_back(0);
    double w = val - (double)idc[1];
    ws[3] = (1.0 / 6.0) * w * w * w;
    ws[0] = (1.0 / 6.0) + (1.0 / 2.0) * w * (w - 1.0) - ws[3];
    ws[2] = w + ws[0] - 2.0 * ws[3];
    ws[1] = 1.0 - ws[0] - ws[2] - ws[3];
    return ws;
}

vector<double> Thevenaz::applyValue5(double val, vector<int> idc, int weight_length)
{
    vector<double> ws;
    for (int i = 0; i < weight_length; ++i)
        ws.push_back(0);
    double w = val - (double)idc[2];
    double w2 = w * w;
    ws[5] = (1.0 / 120.0) * w * w2 * w2;
    w2 -= w;
    double w4 = w2 * w2;
    w -= 1.0 / 2.0;
    double t = w2 * (w2 - 3.0);
    ws[0] = (1.0 / 24.0) * (1.0 / 5.0 + w2 + w4) - ws[5];
    double t0 = (1.0 / 24.0) * (w2 * (w2 - 5.0) + 46.0 / 5.0);
    double t1 = (-1.0 / 12.0) * w * (t + 4.0);
    ws[2] = t0 + t1;
    ws[3] = t0 - t1;
    t0 = (1.0 / 16.0) * (9.0 / 5.0 - t);
    t1 = (1.0 / 24.0) * w * (w4 - w2 - 5.0);
    ws[1] = t0 + t1;
    ws[4] = t0 - t1;
    return ws;
}

vector<double> Thevenaz::applyValue7(double val, vector<int> idc, int weight_length)
{
    vector<double> ws;
    for (int i = 0; i < weight_length; ++i)
        ws.push_back(0);
    double w = val - (double)idc[3];
    ws[0] = 1.0 - w;
    ws[0] *= ws[0];
    ws[0] *= ws[0] * ws[0];
    ws[0] *= (1.0 - w) / 5040.0;
    double w2 = w * w;
    ws[1] = (120.0 / 7.0 + w * (-56.0 + w * (72.0 + w * (-40.0 + w2 * (12.0 + w * (-6.0 + w)))))) / 720.0;
    ws[2] = (397.0 / 7.0 - w * (245.0 / 3.0 + w * (-15.0 + w * (-95.0 / 3.0 + w * (15.0 + w * (5.0 + w * (-5.0 + w))))))) / 240.0;
    ws[3] = (2416.0 / 35.0 + w2 * (-48.0 + w2 * (16.0 + w2 * (-4.0 + w)))) / 144.0;
    ws[4] = (1191.0 / 35.0 - w * (-49.0 + w * (-9.0 + w * (19.0 + w * (-3.0 + w) * (-3.0 + w2))))) / 144.0;
    ws[5] = (40.0 / 7.0 + w * (56.0 / 3.0 + w * (24.0 + w * (40.0 / 3.0 + w2 * (-4.0 + w * (-2.0 + w)))))) / 240.0;
    ws[7] = w2;
    ws[7] *= ws[7] * ws[7];
    ws[7] *= w / 5040.0;
    ws[6] = 1.0 - ws[0] - ws[1] - ws[2] - ws[3] - ws[4] - ws[5] - ws[7];
    return ws;
}

vector<double> Thevenaz::applyValue9(double val, vector<int> idc, int weight_length)
{
    vector<double> ws;
    for (int i = 0; i < weight_length; ++i)
        ws.push_back(0);
    double w = val - (double)idc[4];
    ws[0] = 1.0 - w;
    ws[0] *= ws[0];
    ws[0] *= ws[0];
    ws[0] *= ws[0] * (1.0 - w) / 362880.0;
    ws[1] = (502.0 / 9.0 + w * (-246.0 + w * (472.0 + w * (-504.0 + w * (308.0 + w * (-84.0 + w * (-56.0 / 3.0 + w * (24.0 + w * (-8.0 + w))))))))) / 40320.0;
    ws[2] = (3652.0 / 9.0 - w * (2023.0 / 2.0 + w * (-952.0 + w * (938.0 / 3.0 + w * (112.0 + w * (-119.0 + w * (56.0 / 3.0 + w * (14.0 + w * (-7.0 + w))))))))) / 10080.0;
    ws[3] = (44117.0 / 42.0 + w * (-2427.0 / 2.0 + w * (66.0 + w * (434.0 + w * (-129.0 + w * (-69.0 + w * (34.0 + w * (6.0 + w * (-6.0 + w))))))))) / 4320.0;
    double w2 = w * w;
    ws[4] = (78095.0 / 63.0 - w2 * (700.0 + w2 * (-190.0 + w2 * (100.0 / 3.0 + w2 * (-5.0 + w))))) / 2880.0;
    ws[5] = (44117.0 / 63.0 + w * (809.0 + w * (44.0 + w * (-868.0 / 3.0 + w * (-86.0 + w * (46.0 + w * (68.0 / 3.0 + w * (-4.0 + w * (-4.0 + w))))))))) / 2880.0;
    ws[6] = (3652.0 / 21.0 - w * (-867.0 / 2.0 + w * (-408.0 + w * (-134.0 + w * (48.0 + w * (51.0 + w * (-4.0 + w) * (-1.0 + w) * (2.0 + w))))))) / 4320.0;
    ws[7] = (251.0 / 18.0 + w * (123.0 / 2.0 + w * (118.0 + w * (126.0 + w * (77.0 + w * (21.0 + w * (-14.0 / 3.0 + w * (-6.0 + w * (-2.0 + w))))))))) / 10080.0;
    ws[9] = w2 * w2;
    ws[9] *= ws[9] * w / 362880.0;
    ws[8] = 1.0 - ws[0] - ws[1] - ws[2] - ws[3] - ws[4] - ws[5] - ws[6] - ws[7] - ws[9];
    return ws;
}






