using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class TrainingDummy : MonoBehaviour, IDamagable<DamageObject>{

	public string HitSound;
	public ParticleSystem[] Particles;
	private Animator animator;
	private Vector3 addForce;

	void Start(){
		animator = GetComponent<Animator>();
	}

	public void Hit(DamageObject DO){
		if(animator){

			//запустить анимацию удара
			animator.SetTrigger("Hit");

			//запустить звук удара
			GlobalAudioPlayer.PlaySFXAtPosition(HitSound, transform.position);

			//выбрать силу для следующего fixed update
			int attackDir = (DO.inflictor.transform.position.x < transform.position.x)? -1 : 1; //направление входящей атаки
			addForce = Vector3.right * attackDir * -DO.knockBackForce; //добавить силу в направлении атаки

			//запустить частицы(для анимации)
			foreach(ParticleSystem particles in Particles){
				particles.Play();
			}
		}
	}

	//добавить силу для обновления физики
	void FixedUpdate(){
		if(addForce.magnitude>0){
			GetComponent<Rigidbody>().velocity = addForce;
			addForce = Vector3.zero;
		}
	}
}
