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
            // TODO: ADD CODE HERE
            // Transform the line to the ellipsoid's local coordinate system
            var localLine = new Line(line.X0 - Center, line.Dx);

            // Coefficients of the quadratic equation for intersection calculation
            var a = Math.Pow(localLine.Dx.X, 2) / Math.Pow(SemiAxesLength.X, 2) +
                    Math.Pow(localLine.Dx.Y, 2) / Math.Pow(SemiAxesLength.Y, 2) +
                    Math.Pow(localLine.Dx.Z, 2) / Math.Pow(SemiAxesLength.Z, 2);

            var b = 2 * (localLine.Dx.X * localLine.X0.X / Math.Pow(SemiAxesLength.X, 2) +
                         localLine.Dx.Y * localLine.X0.Y / Math.Pow(SemiAxesLength.Y, 2) +
                         localLine.Dx.Z * localLine.X0.Z / Math.Pow(SemiAxesLength.Z, 2));

            var c = Math.Pow(localLine.X0.X, 2) / Math.Pow(SemiAxesLength.X, 2) +
                    Math.Pow(localLine.X0.Y, 2) / Math.Pow(SemiAxesLength.Y, 2) +
                    Math.Pow(localLine.X0.Z, 2) / Math.Pow(SemiAxesLength.Z, 2) - Radius * Radius;

            // Calculate the discriminant of the quadratic equation
            var discriminant = b * b - 4 * a * c;

            if (discriminant < 0.001)
            {
                // No intersection
                return new Intersection(false, false, this, line, 0, null!);
            }

            // Calculate intersection parameters using quadratic formula
            var t1 = (-b - Math.Sqrt(discriminant)) / (2 * a);
            var t2 = (-b + Math.Sqrt(discriminant)) / (2 * a);

            var validT1 = t1 >= minDist && t1 <= maxDist;
            var validT2 = t2 >= minDist && t2 <= maxDist;

            if (!validT1 && !validT2)
            {
                // No valid intersection points
                return new Intersection(false, false, this, line, 0, null!);
            }

            // Determine the intersection point with the smallest positive t value
            var intersectionT = validT1 ? t1 : t2;
            var intersectionPoint = localLine.CoordinateToPosition(intersectionT) + Center;

            // Calculate the normal vector at the intersection point
            var normal = Normal(intersectionPoint);

            return new Intersection(true, true, this, line, intersectionT, normal);

        }

        public Vector Normal(Vector point)
        {
            var normal = new Vector(
            point.X / Math.Pow(SemiAxesLength.X, 2),
            point.Y / Math.Pow(SemiAxesLength.Y, 2),
            point.Z / Math.Pow(SemiAxesLength.Z, 2)
            );

            return normal.Normalize();
        }
    }
}
