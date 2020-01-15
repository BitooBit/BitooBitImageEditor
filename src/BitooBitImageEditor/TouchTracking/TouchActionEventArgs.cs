using System;
using Xamarin.Forms;

namespace BitooBitImageEditor.TouchTracking
{
    /// <summary>for internal use by <see cref="BitooBitImageEditor"/></summary>
    public class TouchActionEventArgs : EventArgs
    {
#pragma warning disable CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена
        public TouchActionEventArgs(long id, TouchActionType type, Point location, bool isInContact)

        {
            Id = id;
            Type = type;
            Location = location;
            IsInContact = isInContact;
        }

        public long Id { private set; get; }

        public TouchActionType Type { private set; get; }

        public Point Location { private set; get; }

        public bool IsInContact { private set; get; }
#pragma warning restore CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена
    }
}
