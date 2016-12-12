﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

#region Enums
public enum UIState
{
	Play,
	Buy
}
#endregion

public class UIStateManager : MonoBehaviour
{


    public static string state;

	#region Inspector Fields
	[SerializeField]
	[Tooltip("Text component for the gold counter.")]
	/// <summary>
	/// Text component for the gold counter.
	/// </summary>
	private Text _goldCounterText;
	#endregion

	#region Properties
	/// <summary>
	/// Static reference to this instance.
	/// </summary>
	public static UIStateManager Instance;

	/// <summary>
	/// Current state of the UI.
	/// </summary>
	public static UIState State = UIState.Play;
	#endregion

	#region Fields
	// Placing objects.
	/// <summary>
	/// Object currently placing in the room.
	/// </summary>
	private InteractableObject _objectToPlace;
	#endregion


	#region Life Cycle
	private void Awake()
	{
		// Set reference.
		if (Instance == null)
			Instance = this;
	}

	void Start()
	{
		state = "Dig";
		UpdateGoldCounter();
	}

	void Update()
	{
		CheckInput();
	}
	#endregion

	#region Input
	/// <summary>
	/// Checks for any input.
	/// </summary>
	void CheckInput()
	{
		if (EventSystem.current.IsPointerOverGameObject())
			return;

		switch (State)
		{
			case UIState.Buy:
				if (Input.GetMouseButtonDown(0))
				{
					Tile tile = Selector.HitInfo.transform.GetComponent<Tile>();

					// You can only place objects on non-solid tiles.
					if (tile == null || tile.Solid == true)
						break;

					if(tile.AttachObject(_objectToPlace))
					{
						Debug.Log("Placing object.");

						_objectToPlace.Initialize(tile);
						_objectToPlace = null;

						ChangeState(UIState.Play);
					}

				}
				else if (Input.GetMouseButtonDown(1))
				{
					Debug.Log("Cancelling placement.");

					Destroy(_objectToPlace.gameObject);
					_objectToPlace = null;

					ChangeState(UIState.Play);
				}
				else if(Input.GetMouseButtonDown(2) || Input.GetKeyDown(KeyCode.R))
				{
					// Rotate object to place.
					RotateObject();
				}

				break;
		}

	}
	#endregion

	#region Methods
    public void ToggleDigInput(bool val) {
        if(val) StateInput("Dig");
    }
    public void ToggleFillInput(bool val) {
        if(val) StateInput("Fill");
    }
	public void StateInput(string input)
	{
		if (_objectToPlace != null)
			Destroy(_objectToPlace.gameObject);


		ChangeState(UIState.Play);
		state = input;
	}

	/// <summary>
	/// Method to cleanly change the current UI state.
	/// </summary>
	/// <param name="newUIState">New UI state to change to.</param>
	public void ChangeState(UIState newUIState)
	{
		State = newUIState;
	}

	/// <summary>
	/// Start the routine place an object.
	/// </summary>
	/// <param name="objectToPlace">Gameobject to place.</param>
	public void PlaceObject(InteractableObject objectToPlace)
	{
		if (_objectToPlace != null)
			Destroy(_objectToPlace.gameObject);

		// Spawn object to place.
		_objectToPlace = Instantiate(objectToPlace);

		// Change the UI state.
		ChangeState(UIState.Buy);

		// Start the placement coroutine.
		StartCoroutine(PlacingObjectCoroutine());
	}

	void RotateObject(bool clockwise = true)
	{
		if (!_objectToPlace.Rotatable)
			return;

		// Rotate object.
		float angle = (clockwise) ? -90 : 90;
		_objectToPlace.transform.Rotate(Vector3.forward, angle);
	}

	IEnumerator PlacingObjectCoroutine()
	{
		while (State == UIState.Buy && _objectToPlace != null)
		{
			if (Selector.HitInfo.transform != null && Selector.HitInfo.transform.CompareTag("Tile"))
			{
				Vector3 objectPosition = Selector.HitInfo.transform.position + Vector3.back;

				_objectToPlace.transform.position = objectPosition;
			}

			yield return null;
		}
	}
	#endregion

	#region UI
	public void UpdateGoldCounter()
	{
		_goldCounterText.text = GameManager.Gold.ToString();
	}
	#endregion
}
