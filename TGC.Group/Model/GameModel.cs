using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;
using Microsoft.DirectX.Direct3D;
using System.Drawing;
using TGC.Core.Direct3D;
using TGC.Core.Example;
using TGC.Core.Geometry;
using TGC.Core.Input;
using TGC.Core.SceneLoader;
using TGC.Core.Textures;
using TGC.Core.Utils;
using System;
using TGC.Core.BoundingVolumes;
using System.Collections.Generic;
using TGC.Core.SkeletalAnimation;

namespace TGC.Group.Model
{
    public class GameModel : TgcExample
	{
        private TgcScene escenario;
        private TgcSkeletalBoneAttach linterna;
        private TgcSkeletalMesh personaje;
        private TgcBoundingElipsoid boundPersonaje;
        private TgcMesh unMesh;
        private bool flagGod = false;
		private Matrix cameraRotation;
		private float leftrightRot;
		private float updownRot;
		public float RotationSpeed { get; set; }
		private Vector3 viewVector;



		double rot = -21304;
        private bool jumping;
        double variacion;
        private float jumpingElapsedTime;
        private readonly List<Collider> objetosColisionables = new List<Collider>();

        private ElipsoidCollisionManager collisionManager;
        float larg = 4;
       
		public GameModel(string mediaDir, string shadersDir) : base(mediaDir, shadersDir)
        {
            Category = Game.Default.Category;
            Name = Game.Default.Name;
            Description = Game.Default.Description;
        }

		//Caja que se muestra en el ejemplo.
        private TgcBox Box { get; set; }

        //Mesh de TgcLogo.
        private TgcMesh Mesh { get; set; }

        //Boleano para ver si dibujamos el boundingbox
        private bool BoundingBox { get; set; }

        private void seteoDePersonaje() {
            //Cargar personaje con animaciones
            var skeletalLoader = new TgcSkeletalLoader();
            personaje =
                skeletalLoader.loadMeshAndAnimationsFromFile(
                    MediaDir + "SkeletalAnimations\\BasicHuman\\BasicHuman-TgcSkeletalMesh.xml",
                    new[]
                    {
                        MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\Walk-TgcSkeletalAnim.xml",
                        MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\StandBy-TgcSkeletalAnim.xml",
                        MediaDir + "SkeletalAnimations\\BasicHuman\\Animations\\Jump-TgcSkeletalAnim.xml"
                    });
            //IMPORTANTE PREGUNTAR PORQUE DEBERIA ESTAR DESHABILITADO AUTOTRANSFORM
            personaje.AutoTransformEnable = true;
            //Escalarlo porque es muy grande
            personaje.Position = new Vector3(0,-17, 0);
            //Escalamos el personaje ya que sino la escalera es demasiado grande.
            personaje.Scale = new Vector3(1.0f, 1.0f, 1.0f);
            boundPersonaje = new TgcBoundingElipsoid(personaje.BoundingBox.calculateBoxCenter() + new Vector3(0, 0, 0), new Vector3(12, 28, 12));
            jumping = false;
        }
        private void setLinterna() {
            //Crear caja como modelo de Attachment del hueos "Bip01 L Hand"
            linterna = new TgcSkeletalBoneAttach();
            //TgcTexture texturaLinterna = TgcTexture.createTexture(GuiController.Instance.ExamplesMediaDir + "MeshCreator\\Textures\\Vegetacion\\pasto.jpg");
            //box = TgcBox.fromSize(posicionInicial, tamanioBox, pasto);
            var attachmentBox = TgcBox.fromSize(new Vector3(2, 10, 5), Color.Blue);
            linterna.Mesh = attachmentBox.toMesh("attachment");
            linterna.Bone = personaje.getBoneByName("Bip01 L Hand");
            linterna.Offset = Matrix.Translation(8, 0, -10);
            linterna.updateValues(); 
        }
        public override void Init()
        {
            //Device de DirectX para crear primitivas.
            var d3dDevice = D3DDevice.Instance.Device;
            //Seteo el personaje
            seteoDePersonaje();
           // setLinterna();
            //Seteo el escenario
            escenario = new TgcSceneLoader().loadSceneFromFile(MediaDir + "Mapa\\mapa-TgcScene.xml");
			leftrightRot = FastMath.PI_HALF;
			updownRot = -FastMath.PI / 10.0f;
			cameraRotation = Matrix.RotationX(updownRot) * Matrix.RotationY(leftrightRot);
			RotationSpeed = 0.1f;
			//initPuertaGiratoria();   
			//Almacenar volumenes de colision del escenario
			objetosColisionables.Clear();
            foreach (var mesh in escenario.Meshes)
            {
                objetosColisionables.Add(BoundingBoxCollider.fromBoundingBox(mesh.BoundingBox));
            }

            //Crear manejador de colisiones
            collisionManager = new ElipsoidCollisionManager();
            collisionManager.GravityEnabled = true;

        }
 
        private void godMod() {
            Camara = new CamaraGod(personaje.Position,Input);
        }

        private void animacionDePuerta() {
            //Capturar Input Mouse
            if (Input.keyPressed(Key.U))
            {
                //Como ejemplo podemos hacer un movimiento simple de la c�mara.
                //En este caso le sumamos un valor en Y
                ///Camara.SetCamera(Camara.Position + new Vector3(0, 10f, 0), Camara.LookAt);
                //Ver ejemplos de c�mara para otras operaciones posibles.
                unMesh.Position = new Vector3(0, 0, 0);
                unMesh.Rotation = new Vector3(0, System.Convert.ToSingle(rot), 0);
                unMesh.move(new Vector3(System.Convert.ToSingle((larg - (Math.Cos(rot + 3.14) * larg))), 0, System.Convert.ToSingle(Math.Sin(rot + 3.14) * larg)));

                //Si superamos cierto Y volvemos a la posici�n original.
                //if (Camara.Position.Y > 300f)
                // {
                //     Camara.SetCamera(new Vector3(Camara.Position.X, 0f, Camara.Position.Z), Camara.LookAt);
                //  }
            }
            if (rot >= 1.57)
            {
                rot = 1.57;
                variacion = -0.9 * ElapsedTime;
            };
            if (rot <= 0)
            {
                rot = 0;
                variacion = 0.9 * ElapsedTime;
            };
            rot += variacion;
            var ang = System.Convert.ToSingle(rot);
            unMesh.Position = new Vector3(0, 0, 0);
            unMesh.Rotation = new Vector3(0, ang, 0);
            unMesh.move(new Vector3(System.Convert.ToSingle((larg - (Math.Cos(ang) * larg))), 0, System.Convert.ToSingle(Math.Sin(ang) * larg)));
        }
        private void setCamaraPrimeraPersona(Vector3 lookAt) {
            Vector3 posicionConOffset = Vector3.Add(new Vector3(8,20,0),(boundPersonaje.Center));
            Camara.SetCamera(posicionConOffset,lookAt);
        }
        private void moverPersonaje() {
            //seteo de velocidades
            var velocidadCaminar = 1.0f;
			var velocidadRotacion =25;
            var velocidadSalto = 1.0f;
            var tiempoSalto = 1.0f;

            //Calcular proxima posicion de personaje segun Input
            var moveForward = 0f;
            float rotate = 0;
            var moving = false;
            var rotating = false;
            float jump = 0;
			var marchaAtras = false;
            //Adelante
            if (Input.keyDown(Key.W))
            {
                moveForward = -velocidadCaminar;
                moving = true;
            }

            //Atras
            if (Input.keyDown(Key.S))
            {
                moveForward = velocidadCaminar;
                moving = true;
        		marchaAtras = true;

			}

            //Derecha
            if (Input.keyDown(Key.D))
            {
                rotate = velocidadRotacion;
                rotating = true;
		    }

            //Izquierda
            if (Input.keyDown(Key.A))
            {
                rotate = -velocidadRotacion;
                rotating = true;
            }


            //Si hubo rotacion
            if (rotating)
            {
                var rotAngle = Geometry.DegreeToRadian(rotate * ElapsedTime);
                personaje.rotateY(rotAngle);
            }

            else if (moving)
            {
                //Activar animacion de caminando
                personaje.playAnimation("Walk", true);
            }
            //Si no se esta moviendo ni saltando, activar animacion de Parado
            else
            {
                personaje.playAnimation("StandBy", true);
            }


            //Vector de movimiento
            var movementVector = Vector3.Empty;
			var leftrightRotPrevius= leftrightRot-Input.XposRelative * RotationSpeed;
			var updownRotPrevius = updownRot + Input.YposRelative * RotationSpeed;

			if (moving)
            {
                //Aplicar movimiento, desplazarse en base a la rotacion actual del personaje
                movementVector = new Vector3(
                    FastMath.Sin(personaje.Rotation.Y) * moveForward,
                    jump,
                    FastMath.Cos(personaje.Rotation.Y) * moveForward
                    );
				//Se actualiza matrix de rotacion, para no hacer este calculo cada vez y solo cuando en verdad es necesario.
				if(!marchaAtras) viewVector = movementVector; //Solo cambia el vector de view si no esta caminando para atras
			}
			//maximos para los giros del vectorDeView
			if (-1f < leftrightRotPrevius && leftrightRotPrevius <1f ) {leftrightRot-=Input.XposRelative*RotationSpeed;};
			if (-1f < updownRotPrevius && updownRotPrevius < 1f) { updownRot += Input.YposRelative * RotationSpeed;  };
			cameraRotation = Matrix.RotationY(-leftrightRot) * Matrix.RotationX(-updownRot); //calcula la rotacion del vector de view
			var cameraFinalTarget = Vector3.TransformNormal(viewVector, cameraRotation) * 1000; //direccion en que se mueve girada respecto la rotacion de la camara
			Vector3 lookAt = Vector3.Add(boundPersonaje.Center, cameraFinalTarget); //vector lookAt final
			setCamaraPrimeraPersona(lookAt);//se lo paso al setCamara

			  //Actualizar valores de gravedad
            collisionManager.GravityEnabled = true;
            collisionManager.GravityForce = new Vector3(0f, 2f, 0f);

        

            //Mover personaje con detecci�n de colisiones, sliding y gravedad
                //Aca se aplica toda la l�gica de detecci�n de colisiones del CollisionManager. Intenta mover el Elipsoide
                //del personaje a la posici�n deseada. Retorna la verdadera posicion (realMovement) a la que se pudo mover
                var realMovement = collisionManager.moveCharacter(boundPersonaje, movementVector,objetosColisionables);
                personaje.move(realMovement);
            /*
            //Si estaba saltando y hubo colision de una superficie que mira hacia abajo, desactivar salto
            if (jumping && collisionManager.Result.collisionNormal.Y < 0)
            {
                jumping = false;
            }
            */
            /*
            //Actualizar valores de normal de colision
            if (collisionManager.Result.collisionFound)
            {
                collisionNormalArrow.PStart = collisionManager.Result.collisionPoint;
                collisionNormalArrow.PEnd = collisionManager.Result.collisionPoint +
                                            Vector3.Multiply(collisionManager.Result.collisionNormal, 80);

                collisionNormalArrow.updateValues();


                collisionPoint.Position = collisionManager.Result.collisionPoint;
                collisionPoint.updateValues();

            }*/
        }
        public override void Update()
        {
            PreUpdate();
            moverPersonaje();
            //animacionDePuerta();

            if (Input.keyPressed(Key.G)){
                if (!flagGod)
                {
                    godMod();
                    flagGod = true;
                }
                else {
                     Camara.UpdateCamera(ElapsedTime);
                    flagGod = false;
                }
            }
            Camara.UpdateCamera(ElapsedTime);
        }

        private void renderPuerta() {
            unMesh.render();
            unMesh.BoundingBox.render();
            DrawText.drawText(unMesh.Position.ToString(), 0, 50, Color.Red);
        }
        public override void Render()
        {
            //Inicio el render de la escena, para ejemplos simples. Cuando tenemos postprocesado o shaders es mejor realizar las operaciones seg�n nuestra conveniencia.
            PreRender();
            DrawText.drawText("[G]-Habilita GodMod ",0,20, Color.OrangeRed);
            DrawText.drawText("Posicion camara actual: " + TgcParserUtils.printVector3(Camara.Position), 0, 30,Color.OrangeRed);
      
            //renderPuerta();
            personaje.animateAndRender(ElapsedTime);

			foreach (var mesh in escenario.Meshes)
            {
                //Renderizar modelo
                mesh.render();
            }
            /*linterna.Mesh.Enabled = true;
            personaje.Attachments.Add(linterna);
            */
            //Finaliza el render y presenta en pantalla, al igual que el preRender se debe para casos puntuales es mejor utilizar a mano las operaciones de EndScene y PresentScene
            PostRender();
        }

        public override void Dispose()
        {
            escenario.disposeAll();
            personaje.Attachments.Clear();
            personaje.dispose();
        }
    }
}