namespace rt
{
    public class Sphere : Geometry
    {
        private Vector Center { get; set; }
        private double Radius { get; set; }

        public Sphere(Vector center, double radius, Material material, Color color) : base(material, color)
        {
            Center = center;
            Radius = radius;
        }
        // 

        public override Intersection GetIntersection(Line line, double minDist, double maxDist)
        {
            // ADD CODE HERE: Calculate the intersection between the given line and this sphere

            // Calculate coefficients of the quadratic equation for intersection
            var a = line.Dx * line.Dx;
            var b = line.Dx * line.X0 * 2;
            b -= line.Dx * Center * 2;
            var c = (line.X0 * line.X0) + (Center * Center) - (Radius * Radius) - line.X0 * Center * 2;

            // Calculate discriminant
            var discriminant = (b * b) - (a * c * 4.0);

            // Check if the discriminant is less than a small threshold (indicating no real solutions)
            if (discriminant < 0.001)
                return new Intersection(false, false, this, line, 0);

            // Calculate intersection points
            var t1 = (-b - Math.Sqrt(discriminant)) / (2.0 * a);
            var t2 = (-b + Math.Sqrt(discriminant)) / (2.0 * a);

            // Check if the intersection points are within the specified distance range
            var validT1 = t1 >= minDist && t1 <= maxDist;
            var validT2 = t2 >= minDist && t2 <= maxDist;

            // Handle different intersection cases and return the appropriate Intersection object
            if (!validT1 && !validT2)
                return new Intersection(false, false, this, line, 0);
            if (validT1 && !validT2)
                return new Intersection(true, true, this, line, t1);
            if (!validT1)
                return new Intersection(true, true, this, line, t2);

            // Both intersection points are valid, choose the closer one
            var minT = Math.Min(t1, t2);
            return new Intersection(true, true, this, line, minT);
        }


        public override Vector Normal(Vector v)
        {
            var n = v - Center;
            n.Normalize();
            return n;
        }
    }
}