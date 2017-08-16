namespace Zebble
{
    using System;
    using System.Threading.Tasks;

    public class SmoothCompass : IDisposable
    {
        public struct Heading { public float ActualCompass, SmoothValue; }

        const float FULL_CIRCLE = 360;
        const float COMPASS_APPROACH_SPEED = 0.25f;
        const float ZERO_POINT_THRESHOLD = 20;

        bool MotionDetectorsExist, IStartedCompass, IStartedGyroscope, IStartedAccelerometer;
        string RotationVector; // X, Y or Z
        float LatestCompassHeading = -1, InitialCompassValue, TotalRotation, SmoothHeading, CurrentError;

        public float GyroscopeChangeSensitivity { get; set; } = 0.05f;
        public float ToleratedError { get; set; } = 6f;
        public float TooMuchError { get; set; } = 20f;

        public readonly AsyncEvent<Heading> Changed = new AsyncEvent<Heading>();

        private SmoothCompass() { }

        public static async Task<SmoothCompass> Create()
        {
            var result = new SmoothCompass
            {
                MotionDetectorsExist = Device.Gyroscope.IsAvailable() && Device.Accelerometer.IsAvailable()
            };

            if (!Device.Compass.IsActive)
            {
                result.IStartedCompass = true;
                Device.Compass.Changed.Handle(h => result.CompassChanged((float)h));
                await Device.Compass.Start();
            }

            if (result.MotionDetectorsExist) await result.StartMotionDetectors();

            return result;
        }

        async Task StartMotionDetectors()
        {
            if (!Device.Gyroscope.IsActive)
            {
                IStartedGyroscope = true;
                Device.Gyroscope.Changed.Handle(h => GyroscopeChanged(h));
                await Device.Gyroscope.Start();
            }

            if (!Device.Accelerometer.IsActive)
            {
                IStartedAccelerometer = true;
                Device.Accelerometer.Changed.Handle(h => AccelerometerChanged(h));
                await Device.Accelerometer.Start();
            }
        }

        void CompassChanged(float heading)
        {
            var firstTime = LatestCompassHeading == -1;
            LatestCompassHeading = heading;

            if (firstTime || !MotionDetectorsExist)
            {
                SmoothHeading = LatestCompassHeading;
                OnChanged();
            }

            CurrentError = Math.Abs(LatestCompassHeading - SmoothHeading);
        }

        float GetRotation(MotionVector change)
        {
            double rotation;

            switch (RotationVector)
            {
                case "X": rotation = change.X; break;
                case "Y": rotation = change.Y; break;
                case "Z": rotation = change.Z; break;
                default: return 0f;
            }

            if (Math.Abs(rotation) < GyroscopeChangeSensitivity) return 0f;

            return (float)(rotation * (int)SenrorDelay.Game * 0.001f).ToDegreeFromRadians();
        }

        void AccelerometerChanged(MotionVector vector)
        {
            string newRotationVector;

            if (vector.X <= vector.Y && vector.X <= vector.Z) newRotationVector = "X";
            else if (vector.Y <= vector.Z) newRotationVector = "Y";
            else newRotationVector = "Z";

            if (RotationVector == newRotationVector) return;
            else
            {
                RotationVector = newRotationVector;
                Reset();
            }
        }

        float GetHeadingError(float newHeading)
        {
            var error = Math.Abs(LatestCompassHeading - newHeading);

            if (LatestCompassHeading > FULL_CIRCLE - ZERO_POINT_THRESHOLD && newHeading < ZERO_POINT_THRESHOLD)
            {
                error = Math.Abs((FULL_CIRCLE - LatestCompassHeading) - newHeading);
            }
            else if (newHeading > FULL_CIRCLE - ZERO_POINT_THRESHOLD && LatestCompassHeading < ZERO_POINT_THRESHOLD)
            {
                error = Math.Abs((FULL_CIRCLE - newHeading) - LatestCompassHeading);
            }

            return error;
        }

        float GetNewHeading(float change)
        {
            var newHeading = InitialCompassValue + TotalRotation - change;
            while (newHeading < 0) newHeading += FULL_CIRCLE;
            while (newHeading > FULL_CIRCLE) newHeading -= FULL_CIRCLE;

            return newHeading;
        }

        void GyroscopeChanged(MotionVector vector)
        {
            if (LatestCompassHeading == -1) return; // Not started yet. Ignore.

            var change = GetRotation(vector);

            var newHeading = GetNewHeading(change);
            var error = GetHeadingError(newHeading);

            TotalRotation -= change;

            if (error >= TooMuchError) newHeading = LatestCompassHeading;
            else if (error > ToleratedError)
            {
                // Get it closer to the compass reading:

                if (IsAfter(newHeading, LatestCompassHeading))
                {
                    newHeading -= COMPASS_APPROACH_SPEED;
                    TotalRotation -= COMPASS_APPROACH_SPEED;
                }
                else
                {
                    newHeading += COMPASS_APPROACH_SPEED;
                    TotalRotation += COMPASS_APPROACH_SPEED;
                }

                if (newHeading > FULL_CIRCLE) newHeading = FULL_CIRCLE;
                else if (newHeading < 0) newHeading = 0;
            }

            SmoothHeading = newHeading;
            CurrentError = error;

            OnChanged();
        }

        void OnChanged() { Changed.Raise(new Heading { SmoothValue = SmoothHeading, ActualCompass = LatestCompassHeading }); }

        bool IsAfter(float first, float second)
        {
            var diff = second - first;
            if (diff < 0) diff += FULL_CIRCLE;

            return diff > FULL_CIRCLE / 2;
        }

        void Reset()
        {
            if (LatestCompassHeading == -1) return;

            InitialCompassValue = SmoothHeading = LatestCompassHeading;
            TotalRotation = 0;
            CurrentError = 0;
            OnChanged();
        }

        public void Dispose()
        {
            Changed?.Dispose();

            if (IStartedCompass) Device.Compass.Stop();
            if (IStartedGyroscope) Device.Gyroscope.Stop();
            if (IStartedAccelerometer) Device.Accelerometer.Stop();
        }
    }
}