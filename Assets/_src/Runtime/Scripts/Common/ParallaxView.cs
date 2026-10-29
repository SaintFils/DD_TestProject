using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DDTest.Common
{
	public class ParallaxView : MonoBehaviour
	{
		private const float MinOffsetChange = 0.001f;


		[SerializeField]
		private Layer[] _layers;

		[SerializeField, Range(0f, 1f)]
		private float _verticalFactor = 0.5f;

		[SerializeField, Min(0f)]
		private float _smoothTime = 0.2f;


		private Vector2[] _basePositions;
		private Vector2 _offset;
		private Vector2 _appliedOffset;
		private Vector2 _velocity;


		private void Awake()
		{
			_basePositions = new Vector2[_layers.Length];

			for (int index = 0; index < _layers.Length; index++)
				_basePositions[index] = _layers[index].Target.anchoredPosition;

			_offset = GetPointerOffset();
			Apply();
		}

		private void Update()
		{
			_offset = Vector2.SmoothDamp(_offset, GetPointerOffset(), ref _velocity, _smoothTime, Mathf.Infinity, Time.deltaTime);

			if ((_offset - _appliedOffset).sqrMagnitude >= MinOffsetChange * MinOffsetChange)
				Apply();
		}


		private void Apply()
		{
			_appliedOffset = _offset;
			Vector2 shift = new(_offset.x, _offset.y * _verticalFactor);

			for (int index = 0; index < _layers.Length; index++)
				_layers[index].Target.anchoredPosition = _basePositions[index] - shift * _layers[index].Amplitude;
		}

		private static Vector2 GetPointerOffset()
		{
			Pointer pointer = Pointer.current;

			if (pointer == null || Screen.width == 0 || Screen.height == 0)
				return Vector2.zero;

			Vector2 position = pointer.position.ReadValue();
			float x = position.x / Screen.width * 2f - 1f;
			float y = position.y / Screen.height * 2f - 1f;

			return new Vector2(Mathf.Clamp(x, -1f, 1f), Mathf.Clamp(y, -1f, 1f));
		}


		[Serializable]
		private struct Layer
		{
			public RectTransform Target;
			public float Amplitude;
		}
	}
}
