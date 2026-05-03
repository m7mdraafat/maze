/**
 * 3D maze visualization built with React Three Fiber (a React renderer for Three.js).
 *
 * Each grid cell is a rounded box whose color, height and emissive glow change
 * based on its current CellState. Heights are smoothly animated each frame so
 * cells "rise" as the algorithm visits them, giving the search a tangible feel.
 *
 * Color legend (also shown in the UI):
 *   green   = start    red    = goal
 *   dark    = wall     yellow = frontier (discovered, not expanded)
 *   cyan    = visited  purple = final path
 */
import { Canvas, useFrame } from "@react-three/fiber";
import { OrbitControls, RoundedBox, ContactShadows, Environment } from "@react-three/drei";
import { useRef } from "react";
import * as THREE from "three";
import { useMazeStore } from "../stores/maze-store";
import type { CellState } from "../types";

// Visual style for each cell state: base color, emissive glow color, and target height.
const STYLES: Record<CellState, { color: string; emissive: string; height: number }> = {
  empty:    { color: "#1e293b", emissive: "#000000", height: 0.15 },
  wall:     { color: "#0b1220", emissive: "#000000", height: 1.6 },
  start:    { color: "#22c55e", emissive: "#16a34a", height: 1.0 },
  goal:     { color: "#ef4444", emissive: "#b91c1c", height: 1.0 },
  frontier: { color: "#facc15", emissive: "#ca8a04", height: 0.45 },
  visited:  { color: "#06b6d4", emissive: "#0891b2", height: 0.55 },
  path:     { color: "#a855f7", emissive: "#7e22ce", height: 1.2 },
};

function CellBox({ r, c, state }: { r: number; c: number; state: CellState }) {
  const ref = useRef<THREE.Mesh>(null!);
  const matRef = useRef<THREE.MeshStandardMaterial>(null!);
  const style = STYLES[state];

  // useFrame runs once per render frame (~60 fps).
  // We use it to animate the cell's height and pulse its glow without re-rendering React.
  useFrame((_, dt) => {
    if (!ref.current) return;

    // Lerp current height toward target height. dt makes it framerate-independent.
    const cur = ref.current.scale.y;
    ref.current.scale.y = cur + (style.height - cur) * Math.min(1, dt * 8);

    // Pulse the emissive intensity for path/frontier so the search feels "alive".
    if (matRef.current) {
      const t = performance.now() / 400;
      const pulse = state === "path" ? 0.6 + Math.sin(t + r + c) * 0.3
                  : state === "frontier" ? 0.5 + Math.sin(t * 1.5) * 0.2
                  : state === "visited" ? 0.35
                  : state === "start" || state === "goal" ? 0.8
                  : 0;
      matRef.current.emissiveIntensity = pulse;
    }
  });

  return (
    <RoundedBox
      ref={ref as never}
      args={[0.92, 1, 0.92]}
      radius={0.08}
      smoothness={3}
      position={[c, 0.5, r]}
      scale={[1, 0.15, 1]}
      castShadow
      receiveShadow
      onPointerDown={(e) => {
        // Mouse interaction:
        //   plain click  -> toggle a wall
        //   shift+click  -> move the start cell
        //   alt+click    -> move the goal cell
        e.stopPropagation();
        const s = useMazeStore.getState();
        if (e.shiftKey) s.setStart({ row: r, col: c });
        else if (e.altKey) s.setGoal({ row: r, col: c });
        else s.toggleWall(r, c);
      }}
    >
      <meshStandardMaterial
        ref={matRef}
        color={style.color}
        emissive={style.emissive}
        emissiveIntensity={0}
        roughness={0.35}
        metalness={0.2}
      />
    </RoundedBox>
  );
}

export function Grid3D() {
  const { rows, cols, walls, states, start, goal } = useMazeStore();

  // Build the grid: for each (r,c) decide which CellState applies.
  // Order of precedence: start/goal > wall > algorithm state > empty.
  const cells = [];
  for (let r = 0; r < rows; r++)
    for (let c = 0; c < cols; c++) {
      const k = `${r},${c}`;
      let st: CellState = "empty";
      if (walls.has(k)) st = "wall";
      else if (states.has(k)) st = states.get(k)!;
      if (start.row === r && start.col === c) st = "start";
      if (goal.row === r && goal.col === c) st = "goal";
      cells.push(<CellBox key={k} r={r} c={c} state={st} />);
    }

  const size = Math.max(rows, cols);

  return (
    <Canvas
      shadows
      camera={{ position: [cols * 0.6, size * 1.3, rows * 1.3], fov: 45 }}
      gl={{ antialias: true }}
    >
      {/* Dark background + matching fog gives the scene depth. */}
      <color attach="background" args={["#070b1a"]} />
      <fog attach="fog" args={["#070b1a", size * 1.2, size * 3]} />

      {/* Lighting: soft ambient + main directional (with shadows) + an indigo rim light. */}
      <ambientLight intensity={0.35} />
      <directionalLight
        position={[size, size * 1.5, size]}
        intensity={1.2}
        castShadow
        shadow-mapSize-width={2048}
        shadow-mapSize-height={2048}
      />
      <pointLight position={[-size, size, -size]} intensity={0.4} color="#6366f1" />

      {/* HDRI environment for realistic reflections on the cell surfaces. */}
      <Environment preset="city" />

      {/* Center the grid around the world origin. */}
      <group position={[-cols / 2, 0, -rows / 2]}>
        {/* Floor plane that catches shadows. */}
        <mesh rotation={[-Math.PI / 2, 0, 0]} position={[cols / 2 - 0.5, 0, rows / 2 - 0.5]} receiveShadow>
          <planeGeometry args={[cols + 4, rows + 4]} />
          <meshStandardMaterial color="#0a0f24" roughness={1} metalness={0} />
        </mesh>

        {/* Soft contact shadow underneath the cells. */}
        <ContactShadows
          position={[cols / 2 - 0.5, 0.01, rows / 2 - 0.5]}
          opacity={0.6}
          scale={size * 2}
          blur={2}
          far={4}
        />

        {cells}
      </group>

      {/* Mouse-driven camera. Damping + clamps prevent flipping under the floor. */}
      <OrbitControls
        target={[0, 0, 0]}
        enableDamping
        dampingFactor={0.08}
        maxPolarAngle={Math.PI / 2.1}
        minDistance={size * 0.6}
        maxDistance={size * 3}
      />
    </Canvas>
  );
}
