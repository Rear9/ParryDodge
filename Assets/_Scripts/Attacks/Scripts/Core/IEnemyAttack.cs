using UnityEngine;

public interface IEnemyAttack // common point for spawning systems to call any attack without knowing their type
{
  void InitAttack(Transform player);
}
