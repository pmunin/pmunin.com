import * as THREE from '//cdn.skypack.dev/three@0.129?min';
import {
    OrbitControls
} from '//cdn.skypack.dev/three@0.129.0/examples/jsm/controls/OrbitControls?min';
import {
    EffectComposer
} from '//cdn.skypack.dev/three@0.129.0/examples/jsm/postprocessing/EffectComposer?min';
import {
    RenderPass
} from '//cdn.skypack.dev/three@0.129.0/examples/jsm/postprocessing/RenderPass?min';
import {
    BokehPass
} from '//cdn.skypack.dev/three@0.129.0/examples/jsm/postprocessing/BokehPass?min';
import {
    Pane
} from '//cdn.skypack.dev/tweakpane@3.0.2?min';
import {
    gsap
} from '//cdn.skypack.dev/gsap@3.6.1?min';

// ----
// const
// ----

const CH = 48; // cyl height
const CR = 44; // cyl radius
const IR = 0.525; // ico radius
const FO = 0.1; // focus

// ----
// main
// ----

const renderer = new THREE.WebGLRenderer();
const scene = new THREE.Scene();
const camera = new THREE.PerspectiveCamera(75, 2, .1, 100);
const controls = new OrbitControls(camera, renderer.domElement);


scene.background = new THREE.Color('white');
camera.position.set(0, 0, 4);
controls.enableDamping = true;
renderer.shadowMap.enabled = true;

{
    const light = new THREE.DirectionalLight('white', .9);
    light.castShadow = true;
    light.position.set(0, CH, 0);
    light.shadow.camera.far = CH * 2;
    light.shadow.mapSize.setScalar(1024);
    scene.add(light);
}

scene.add(new THREE.AmbientLight('white', 1));

const names = Object.keys(THREE.Color.NAMES);

const geom = new THREE.IcosahedronGeometry(IR, 8);
const mat = new THREE.MeshPhongMaterial();
const mesh = new THREE.InstancedMesh(geom, mat, names.length);
for (const [i, k] of names.entries()) {
    const c = new THREE.Color(k);
    const hsl = {};
    c.getHSL(hsl);
    const a = hsl.h * Math.PI * 2;
    const r = hsl.s * CR;
    const h = (hsl.l - .5) * CH;
    const m = new THREE.Matrix4();
    m.setPosition(r * Math.sin(a), h, r * Math.cos(a));
    mesh.setMatrixAt(i, m);
    mesh.setColorAt(i, c);
}
mesh.instanceMatrix.needsUpdate = true;
mesh.instanceColor.needsUpdate = true;
mesh.castShadow = true;
mesh.receiveShadow = true;
scene.add(mesh);

// ----
// render
// ----

const composer = new EffectComposer(renderer);
composer.addPass(new RenderPass(scene, camera));
composer.addPass(new BokehPass(scene, camera, {
    focus: FO,
    maxblur: 0.005,
    aperture: 0.005
}));

renderer.setAnimationLoop(() => {
    composer.render();
    controls.update();
});

// ----
// view
// ----

function resize(w, h, dpr = devicePixelRatio) {
    renderer.setPixelRatio(dpr);
    renderer.setSize(w, h, false);
    composer.setPixelRatio(dpr);
    composer.setSize(w, h);
    camera.aspect = w / h;
    camera.updateProjectionMatrix();
}
addEventListener('resize', () => resize(innerWidth, innerHeight));
dispatchEvent(new Event('resize'));
document.body.prepend(renderer.domElement);


function cameraTo(idx, duration) {
    const m = new THREE.Matrix4();
    mesh.getMatrixAt(idx, m);
    const p = new THREE.Vector3(); // mesh pos
    p.setFromMatrixPosition(m);
    const dir = p.clone().normalize();
    const p1 = p.add(dir.multiplyScalar(FO + IR)); // next cam pos
    gsap.killTweensOf(camera.position, 'x,y,z');
    gsap.to(camera.position, {
        x: p1.x,
        y: p1.y,
        z: p1.z + 1,
        duration: duration
    });
}


function startAnimation() {
    const duration = 10 + Math.floor(Math.random() * 30) //10-40 secs
    const color = Math.floor(Math.random() * 130);
    cameraTo(color, duration);
    setTimeout(() => {
        startAnimation();
    }, duration * 1000)
}

startAnimation();