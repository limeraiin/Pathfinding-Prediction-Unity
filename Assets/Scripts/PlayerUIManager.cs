using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] private Image _energyBar;

    [SerializeField]private MovementController _playerMovement;



    private void Update()
    {
        UpdateEnergyBar();
    }

    private void UpdateEnergyBar()
    {
        _energyBar.fillAmount = _playerMovement.PredictedEnergy/_playerMovement.MaxEnergy;
    }
}
