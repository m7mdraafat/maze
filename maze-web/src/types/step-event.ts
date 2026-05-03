import type { Cell } from "./cell";

export type StepEvent = {
  type: "visit" | "frontier" | "path" | "done";
  cell: Cell;
  g?: number; h?: number; f?: number;
};
