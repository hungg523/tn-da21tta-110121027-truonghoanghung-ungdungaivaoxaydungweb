import { Injectable } from '@angular/core';

interface Scripts {
  name: string;
  src: string;
}

interface ScriptLoadResult {
  script: string;
  loaded: boolean;
  status: string;
}

export const ScriptStore: Scripts[] = [
  { name: 'custom', src: 'assets/js/custom.js' }
];

@Injectable({
  providedIn: 'root'
})
export class ScriptService {
  private scripts: { [key: string]: { loaded: boolean; src: string } } = {};

  constructor() {
    ScriptStore.forEach((script: Scripts) => {
      this.scripts[script.name] = {
        loaded: false,
        src: script.src
      };
    });
  }

  load(...scripts: string[]): Promise<ScriptLoadResult[]> {
    const promises: Promise<ScriptLoadResult>[] = [];
    scripts.forEach((script) => promises.push(this.loadScript(script)));
    return Promise.all(promises);
  }

  private loadScript(name: string): Promise<ScriptLoadResult> {
    return new Promise((resolve, reject) => {
      if (this.scripts[name].loaded) {
        resolve({ script: name, loaded: true, status: 'Already Loaded' });
      } else {
        const script = document.createElement('script');
        script.type = 'text/javascript';
        script.src = this.scripts[name].src;
        script.onload = () => {
          this.scripts[name].loaded = true;
          resolve({ script: name, loaded: true, status: 'Loaded' });
        };
        script.onerror = (error: any) => resolve({ script: name, loaded: false, status: 'Loaded' });
        document.getElementsByTagName('head')[0].appendChild(script);
      }
    });
  }
} 