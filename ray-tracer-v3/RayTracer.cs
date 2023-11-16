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

        // Useless??
        private bool IsLit(Vector point, Light light)
        {
            // TODO: ADD CODE HERE

            Vector lightDirection = (light.Position - point).Normalize();
            double distanceToLight = lightDirection.Length();

            lightDirection.Normalize();
            Line ray = new Line(point, lightDirection);

            Intersection intersection = FindFirstIntersection(ray, 0, distanceToLight);

            return !(intersection.Valid && intersection.T < distanceToLight);
        }

        private Color ColorOfPixel(Intersection intersection, Camera camera, Light[] lights)
        {
            Color color = new();

            foreach (var light in lights)
            {
                Vector lightPosition = light.Position;
                // If not working, revert the above values
                Vector fromCameraToIntersection = (intersection.Position - camera.Position).Normalize();
                Vector normalToSurface = intersection.Geometry.Normal(intersection.Position).Normalize();
                Vector fromLightToIntersection = (intersection.Position - lightPosition).Normalize();
                Vector result = (normalToSurface * (normalToSurface * fromLightToIntersection) * 2 - fromLightToIntersection).Normalize();

                color = intersection.Geometry.Material.Ambient * light.Ambient;

                if (normalToSurface * fromLightToIntersection > 0)
                {
                    color += intersection.Geometry.Material.Diffuse * light.Diffuse *
                                (normalToSurface * fromLightToIntersection);
                }
                if (fromCameraToIntersection * result > 0)
                {
                    color += intersection.Geometry.Material.Specular * light.Specular *
                                Math.Pow(result * fromCameraToIntersection, intersection.Geometry.Material.Shininess);
                }
                color *= light.Intensity * 5;
            }
            return color;
        }

        public void Render(Camera camera, int width, int height, string filename)
        {
            var image = new Image(width, height);
            var viewParallel = (camera.Up ^ camera.Direction).Normalize();

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    double x = ImageToViewPlane(i, width, camera.ViewPlaneWidth);
                    double y = ImageToViewPlane(j, height, camera.ViewPlaneHeight);

                    Vector rayDirection = camera.Direction * camera.ViewPlaneDistance +
                                viewParallel * x +
                                camera.Up * y;
                    Intersection intersection = FindFirstIntersection(new Line(camera.Position, camera.Position + rayDirection),
                                                                        camera.FrontPlaneDistance, camera.BackPlaneDistance);
                    if (intersection.Valid)
                    {
                        image.SetPixel(i, j, ColorOfPixel(intersection, camera, lights));
                    }
                }
            }

            // Store the rendered image
            image.Store(filename);
        }
    }
}