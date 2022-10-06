// MathLibrary.cpp : Defines the exported functions for the DLL.
#include "pch.h" // use stdafx.h in Visual Studio 2017 and earlier
#include <utility>
#include <string>
#include <vector>
#include <limits.h>
#include "DensityMatrix.h"
#include "Ccp4File.h"
#include "SpaceTransformation.h"
#include "Interpolator.h"

using namespace std;

// DLL internal state variables:
static string pdbcode_;  // protein structure code
static string filepath_;  // file location
static vector<double> matrix_;
static vector<vector<double>> slice_density_;
static vector<vector<double>> slice_radiant_;
static vector<vector<double>> slice_laplacian_;
static Ccp4File* cp4_;
static Interpolator* interpMap_;


// Initialize a Fibonacci relation sequence
// such that F(0) = a, F(1) = b.
// This function must be called before any other function.
bool density_init(     
    const char* pdbcode,
    const char* filepath)
{
    if (pdbcode != pdbcode_)
    {
        pdbcode_ = (string)pdbcode;
        matrix_.clear();        
    }
    else
    {
        return true;
    }    
}
void density_add(double val)
{
    matrix_.push_back(val);
}

void density_calc()
{
    cp4_ = new Ccp4File(matrix_);        
}

void density_words(
    int w01_NC, int w02_NR, int w03_NS, int w04_Mode,
    int w05_NCSTART, int w06_NRSTART, int w07_NSSTART,
    int w08_NX, int w09_NY, int w10_NZ,
    double w11_CELLA_X, double w12_CELLA_Y, double w13_CELLA_Z,
    double w14_CELLB_X, double w15_CELLB_Y, double w16_CELLB_Z,
    int w17_MAPC, int w18_MAPR, int w19_MAPS)
{
    cp4_->setWords(w01_NC, w02_NR, w03_NS, w04_Mode,
        w05_NCSTART, w06_NRSTART, w07_NSSTART,
        w08_NX, w09_NY, w10_NZ,
        w11_CELLA_X, w12_CELLA_Y, w13_CELLA_Z,
        w14_CELLB_X, w15_CELLB_Y, w16_CELLB_Z,
        w17_MAPC, w18_MAPR, w19_MAPS);

    interpMap_ = new Thevenaz(matrix_, cp4_->W01_NC, cp4_->W02_NR, cp4_->W03_NS);
    //interpMap_ = new Nearest(matrix_, cp4_->W01_NC, cp4_->W02_NR, cp4_->W03_NS);

}

void create_slice(double cx, double cy, double cz, double lx, double ly, double lz, double px, double py, double pz, double width, double gap)
{
    slice_density_.clear();
    slice_radiant_.clear();
    slice_laplacian_.clear();
    int nums = int(width / gap) + 1;
    int halfLength = (nums-1) / 2;
    
    VectorThree central(cx, cy, cz);
    VectorThree linear(lx, ly, lz);
    VectorThree planar(px, py, pz);
    SpaceTransformation space(central, linear, planar);
    
    for (int i = -1 * halfLength; i <= halfLength; ++i)
    {
        vector<double> row_d;
        vector<double> row_r;
        vector<double> row_l;
        for (int j = -1 * halfLength; j <= halfLength; ++j)
        {
            double x0 = (i * gap);
            double y0 = (j * gap);
            double z0 = 0;
            VectorThree transformed = space.applyTransformation(VectorThree(x0, y0, z0));            
            VectorThree crs = cp4_->getCRSFromXYZ(transformed);
            double density = interpMap_->getValue(crs.A, crs.B, crs.C);
            row_d.push_back(density);
            double radiant = interpMap_->getRadiant(crs.A, crs.B, crs.C);
            row_r.push_back(radiant);
            double laplacian = interpMap_->getLaplacian(crs.A, crs.B, crs.C);
            row_l.push_back(laplacian);
        }
        slice_density_.push_back(row_d);
        slice_radiant_.push_back(row_r);
        slice_laplacian_.push_back(row_l);
    }    
}

double get_slice_value(unsigned int a, unsigned int b)
{
    if (a < slice_density_.size())
    {
        if (b < slice_density_[a].size())
        {
            return slice_density_[a][b];
        }
    }
    return -1;
}
double get_slice_radiant_value(unsigned int a, unsigned int b)
{
    if (a < slice_radiant_.size())
    {
        if (b < slice_radiant_[a].size())
        {
            return slice_radiant_[a][b];
        }
    }
    return -1;
}
double get_slice_laplacian_value(unsigned int a, unsigned int b)
{
    if (a < slice_laplacian_.size())
    {
        if (b < slice_laplacian_[a].size())
        {
            return slice_laplacian_[a][b];
        }
    }
    return -1;
}

