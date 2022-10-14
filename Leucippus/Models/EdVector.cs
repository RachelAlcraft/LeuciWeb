namespace Leucippus.Models
{
    /*public class EdVector
    {
        
        // This class is specially for 3d coordinates
        
        public double[] vector = new double[4];
        public EdVector(Double vx, Double vy, Double vz)
        {
            vector[0] = vx;
            vector[1] = vy;
            vector[2] = vz;
            vector[3] = 0;//space for a value at this point
        }

        public EdVector()
        {
        }

        public double x
        {
            get
            {
                return vector[0];
            }
            set
            {
                vector[0] = value;
            }
        }
        public double y
        {
            get
            {
                return vector[1];
            }
            set
            {
                vector[1] = value;
            }
        }
        public double z
        {
            get
            {
                return vector[2];
            }
            set
            {
                vector[2] = value;
            }
        }
        public double v
        {
            get
            {
                return vector[3];
            }
            set
            {
                vector[3] = value;
            }
        }

        public EdVector findOrthogonalToLine(EdVector line, EdVector plane)
        {
            EdVector O1 = findOrthogonalToPlane(this, line, plane);
            EdVector V1 = new EdVector(this.x + O1.x, this.y + O1.y, this.z + O1.z);
            EdVector O2 = findOrthogonalToPlane(this, line, V1);
            return O2;
        }
        public EdVector findOrthogonalToPlane(EdVector A, EdVector B, EdVector C)
        {
            EdVector AB = new EdVector(B.x - A.x, B.y - A.y, B.z - A.z);
            EdVector AC = new EdVector(C.x - A.x, C.y - A.y, C.z - A.z);
            EdVector O = AB.crossProduct(AC);
            O.normalise();
            return O;
        }

        public EdVector crossProduct(EdVector B)
        {
            double px = (y * B.z) - (z * B.y);
            double py = (z * B.x) - (x * B.z);
            double pz = (x * B.y) - (y * B.x);
            return new EdVector(px, py, pz);
        }

        public EdVector subtract(EdVector B)
        {
            double px = x - B.x;
            double py = y - B.y;
            double pz = z - B.z;
            return new EdVector(px, py, pz);
        }
        public EdVector add(EdVector B)
        {
            double px = x + B.x;
            double py = y + B.y;
            double pz = z + B.z;
            return new EdVector(px, py, pz);
        }

        public EdVector scale(double val)
        {
            double px = x * val;
            double py = y * val;
            double pz = z * val;
            return new EdVector(px, py, pz);
        }

        public double dotProduct(EdVector B)
        {
            double px = x * B.x;
            double py = y * B.y;
            double pz = z * B.z;
            return px + py + pz;
        }

        public double distance(EdVector B)
        {
            double px = Math.Pow(x - B.x, 2);
            double py = Math.Pow(y - B.y, 2);
            double pz = Math.Pow(z - B.z, 2);
            return Math.Sqrt(px + py + pz);
        }

        public double magnitude()
        {
            double px = Math.Pow(x, 2);
            double py = Math.Pow(y, 2);
            double pz = Math.Pow(z, 2);
            return Math.Sqrt(px + py + pz);
        }



        public void normalise()
        {
            Double mag = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2) + Math.Pow(z, 2));
            x /= mag;
            y /= mag;
            z /= mag;
        }

        public override string ToString()
        {
            string str = "(" + Convert.ToString(Math.Round(x, 2)) + "," + Convert.ToString(Math.Round(y, 2)) + "," + Convert.ToString(Math.Round(z, 2)) + ")";
            return str;
        }


    }*/
}
