using System.Diagnostics.CodeAnalysis;

namespace EBCEYS.WindowsServiceHelper
{
    /// <summary>
    /// The wait for status info.
    /// </summary>
    /// <param name="waitForStatus">Indicates should method wait service to change status.</param>
    /// <param name="timeout">Time that thread would be blocked while waiting status.</param>
    public struct WaitForStatusInfo(bool waitForStatus = true, TimeSpan? timeout = null)
    {
        public bool shouldWaitForStatus = waitForStatus;
        public TimeSpan? waitTime = timeout ?? TimeSpan.FromSeconds(10.0);
        public static WaitForStatusInfo Default => new();
        public override readonly bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is WaitForStatusInfo waitFor)
            {
                return waitFor.shouldWaitForStatus == this.shouldWaitForStatus && waitFor.waitTime == waitTime;
            }
            return false;
        }

        public static bool operator ==(WaitForStatusInfo left, WaitForStatusInfo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(WaitForStatusInfo left, WaitForStatusInfo right)
        {
            return !(left == right);
        }

        public override readonly int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
