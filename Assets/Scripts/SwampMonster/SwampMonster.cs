using UnityEngine;
using UnityEngine.Events;

public class SwampMonster : MonoBehaviour {
    [SerializeField] private Animator monsterAnimator;
    [SerializeField] private UnityEvent onMonsterActivation;
    [SerializeField] private Vector3 monsterActivationOffset;
}