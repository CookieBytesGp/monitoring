import { ToolModel } from '../../models/PageBuilder/ToolModel';

export interface ToolState {
  tools: ToolModel[];
  selectedTool: ToolModel | null;
  loading: boolean;
  error: string | null;
}

export const initialToolState: ToolState = {
  tools: [],
  selectedTool: null,
  loading: false,
  error: null,
};