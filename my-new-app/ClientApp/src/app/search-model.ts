import {FileModel} from "./file-model";

export class SearchModel{
  text: string;
  files: FileModel[];
  corruptFiles: FileModel[];
}
