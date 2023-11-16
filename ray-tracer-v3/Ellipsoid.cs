namespace rt
{
    public class Ellipsoid : Geometry
    {
        private Vector Center { get; }
        private Vector SemiAxesLength { get; }
        private double Radius { get; }


        public Ellipsoid(Vector center, Vector semiAxesLength, double radius, Material material, Color color) : base(material, color)
        {
            Center = center;
            SemiAxesLength = semiAxesLength;
            Radius = radius;
        }

        public Ellipsoid(Vector center, Vector semiAxesLength, double radius, Color color) : base(color)
        {
            Center = center;
            SemiAxesLength = semiAxesLength;
            Radius = radius;
        }

        public override Intersection GetIntersection(Line line, double minDist, double maxDist)
        {
            // Semi axes of lengths A, B, C
            var A = SemiAxesLength.X;
            var B = SemiAxesLength.Y;
            var C = SemiAxesLength.Z;

            // Center of the ellipsoid
            var h = Center.X;
            var k = Center.Y;
            var l = Center.Z;

            // Direction vector of the line
            var a = line.Dx.X;
            var c = line.Dx.Y;
            var e = line.Dx.Z;

            // Point on the line
            var b = line.X0.X;
            var d = line.X0.Y;
            var f = line.X0.Z;

            // Made this for ease of writing
            static double pow(double exp) => Math.Pow(exp, 2);

            double aEcuation = pow(a) * pow(B) * pow(C) + pow(c) * pow(A) * pow(C) + pow(e) * pow(A) * pow(B);
            double bEcuation = 2 * (pow(B) * pow(C) * a * (b - h) + c * pow(A) * pow(C) * (d - k) + e * pow(A) * pow(B) * (f - l));
            double cEcuation = (pow(B) * pow(C) * pow(b - h)) + (pow(A) * pow(C) * pow(d - k)) + (pow(A) * pow(B) * pow(f - l)) - pow(Radius * A * B * C);

            var discriminant = bEcuation * bEcuation - 4 * aEcuation * cEcuation;

            if (discriminant < 0)
            {
                return new();
            }
            var t1 = (-bEcuation + Math.Sqrt(discriminant)) / (2 * aEcuation);
            var t2 = (-bEcuation - Math.Sqrt(discriminant)) / (2 * aEcuation);

            if (t1 <= t2)
            {
                if (t1 >= minDist && t1 <= maxDist)
                {
                    Vector vector = line.X0 + line.Dx * t1;
                    var normal = vector - Center;
                    normal.Divide(SemiAxesLength);
                    normal = normal.Normalize();

                    return new Intersection(true, true, this, line, t1, normal);
                }
            }
            if (t2 >= minDist && t2 <= maxDist)
            {
                Vector vector = line.X0 + line.Dx * t2;
                var normal = vector - Center;
                normal.Divide(SemiAxesLength);
                normal = normal.Normalize();

                return new Intersection(true, true, this, line, t2, normal);
            }
            return new();
        }



        public override Vector Normal(Vector point)
        {
            return (point - Center).Normalize();
        }

    }
}
