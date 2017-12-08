namespace Paranothing
{
    interface Lockable
    {
        void lockObj(); // Can't use "lock" as the name of the method.
        void unlockObj();
        bool isLocked();
        void setKeyName(string keyName);
    }
}
