using System;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SpsDocumentGenerator.Base;

/// <summary>
///     Enum 그룹에서 선택 기능을 위한 클래스
/// </summary>
/// <remarks>
///     공통
/// </remarks>
public class EnumSelection<T> : ObservableRecipient where T : Enum, IComparable, IFormattable, IConvertible
{
    /// <summary>
    ///     공백 값 필드
    /// </summary>
    private readonly T _blankValue; // what is considered the "blank" value if it can be deselected?

    /// <summary>
    ///     Broadcast 여부 필드
    /// </summary>
    private readonly bool _broadcast;

    /// <summary>
    ///     선택 가능 여부 필드
    /// </summary>
    private readonly bool _canDeselect; // Can be deselected? (Radio buttons cannot deselect, checkboxes can)

    /// <summary>
    ///     Enum이 Flag인지 여부 필드
    /// </summary>
    private readonly bool _isFlagged; // Enum uses flags?

    /// <summary>
    ///     값 필드
    /// </summary>
    private T _value; // stored value of the Enum


    public EnumSelection(T value, bool broadcast = false) : this(value, false, default, broadcast)
    {
    }

    public EnumSelection(T value, bool canDeselect, bool broadcast = false) : this(value, canDeselect, default, broadcast)
    {
    }

    public EnumSelection(T value, T blankValue, bool broadcast = false) : this(value, true, blankValue, broadcast)
    {
    }

    public EnumSelection(T value, bool canDeselect, T blankValue, bool broadcast = false)
    {
        if (!typeof(T).IsEnum)
            throw new ArgumentException($"{nameof(T)} must be an enum type");
        _isFlagged = typeof(T).IsDefined(typeof(FlagsAttribute), false);

        _value = value;
        _canDeselect = canDeselect;
        _blankValue = blankValue;
        _broadcast = broadcast;

        if (_broadcast) IsActive = true;
    }

    /// <summary>
    ///     값 속성
    /// </summary>
    public T Value
    {
        get => _value;
        set
        {
            if (SetProperty(ref _value, value, _broadcast)) OnPropertyChanged("Item[]");
        }
    }

    // /// <summary>
    // ///     값 속성
    // /// </summary>
    // public T Value
    // {
    //     get => _value;
    //     set
    //     {
    //         if (_value.Equals(value)) return;
    //         _value = value;
    //         OnPropertyChanged();
    //         OnPropertyChanged("Item[]"); // Notify that the indexer property has changed
    //     }
    // }

    /// <summary>
    ///     인덱서 속성
    /// </summary>
    [IndexerName("Item")]
    public bool this[T key]
    {
        get
        {
            var iKey = (int)(object)key;
            return _isFlagged ? ((int)(object)_value & iKey) == iKey : _value.Equals(key);
        }
        set
        {
            if (_isFlagged)
            {
                var iValue = (int)(object)_value;
                var iKey = (int)(object)key;

                if ((iValue & iKey) == iKey == value) return;

                if (value)
                    Value = (T)(object)(iValue | iKey);
                else
                    Value = (T)(object)(iValue & ~iKey);
            }
            else
            {
                if (_value.Equals(key) == value) return;
                if (!value && !_canDeselect) return;

                Value = value ? key : _blankValue;
            }

            OnPropertyChanged("Item[]"); // Notify that the indexer property has changed
        }
    }
}