import { PageModel } from '../../models/PageBuilder/PageModel';

export interface PageState {
  pages: PageModel[];
  selectedPage: PageModel | null;
  loading: boolean;
  error: string | null;
}

export const initialPageState: PageState = {
  pages: [],
  selectedPage: null,
  loading: false,
  error: null,
};