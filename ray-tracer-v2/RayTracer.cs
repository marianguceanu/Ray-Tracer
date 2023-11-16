namespace rt
{
    class RayTracer
    {
        private Geometry[] geometries;
        private Light[] lights;

        public RayTracer(Geometry[] geometries, Light[] lights)
        {
            this.geometries = geometries;
            this.lights = lights;
        }

        private double ImageToViewPlane(int n, int imgSize, double viewPlaneSize)
        {
            return -n * viewPlaneSize / imgSize + viewPlaneSize / 2;
        }

        private Intersection FindFirstIntersection(Line ray, double minDist, double maxDist)
        {
            var intersection = new Intersection();

            foreach (var geometry in geometries)
            {
                var intr = geometry.GetIntersection(ray, minDist, maxDist);

                if (!intr.Valid || !intr.Visible) continue;

                if (!intersection.Valid || !intersection.Visible)
                {
                    intersection = intr;
                }
                else if (intr.T < intersection.T)
                {
                    intersection = intr;
                }
            }

            return intersection;
        }

        private bool IsLit(Vector point, Light light)
        {
            // TODO: ADD CODE HERE
            // Creates a line from light to the point
            var line = new Line(light.Position, point);
            // Finds the first intersection along the line
            var intersection = FindFirstIntersection(line, 0, 1000000);
            // If no intersection or if intersection is farther than the light source, the point is lit
            if (!intersection.Valid || !intersection.Visible)
                return true;
            return intersection.T > (light.Position - point).Length() - 0.001;
        }

        public void Render(Camera camera, int width, int height, string filename)
        {
            var background = new Color();
            var image = new Image(width, height);

            // Iterates over each pixel in the image
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    // TODO: ADD CODE HERE
                    // Calculates 3D point on the view plane corresponding to the current pixel
                    var pointOnViewPlane = camera.Position + camera.Direction * camera.ViewPlaneDistance +
                                          (camera.Up ^ camera.Direction) * ImageToViewPlane(i, width, camera.ViewPlaneWidth) +
                                          camera.Up * ImageToViewPlane(j, height, camera.ViewPlaneHeight);
                    // Constructs a ray from camera position to the calculated point
                    var ray = new Line(camera.Position, pointOnViewPlane);
                    // Finds the first intersection along the ray within specified distance range
                    var intersection = FindFirstIntersection(ray, camera.FrontPlaneDistance, camera.BackPlaneDistance);
                    // If valid and visible intersection exists, calculates pixel color based on lighting
                    if (intersection.Valid && intersection.Visible)
                    {
                        var color = new Color();
                        foreach (var light in lights)
                        {
                            var colorFromLight = new Color();
                            colorFromLight += intersection.Geometry.Material.Ambient * light.Ambient;
                            if (IsLit(intersection.Position, light))
                            {
                                // Calculates diffuse and specular reflections and adds them to colorFromLight
                                var v = intersection.Position;
                                var e = (camera.Position - v).Normalize();
                                var n = ((Ellipsoid)intersection.Geometry).Normal(intersection.Position);
                                var t = (light.Position - v).Normalize();
                                var r = (n * (n * t) * 2 - t).Normalize();
                                if (n * t > 0)
                                    colorFromLight += intersection.Geometry.Material.Diffuse * light.Diffuse * (n * t);
                                if (e * r > 0)
                                    colorFromLight += intersection.Geometry.Material.Specular * light.Specular *
                                                      Math.Pow(e * r, intersection.Geometry.Material.Shininess);
                                colorFromLight *= light.Intensity;
                            }
                            // Multiplies colorFromLight by light intensity and adds to total color
                            color += colorFromLight;
                        }
                        // Sets the pixel color in the image
                        image.SetPixel(i, j, color);
                    }
                    else image.SetPixel(i, j, background);
                }
            }
            // Stores the rendered image to the specified file
            image.Store(filename);
        }
    }
}