import { Grid3D } from "./scene/Grid3D";
import { Controls } from "./ui/Controls";

export default function App() {
  return (
    <div className="w-screen h-screen relative bg-gradient-to-br from-slate-900 to-indigo-950">
      <Grid3D />
      <Controls />
    </div>
  );
}
