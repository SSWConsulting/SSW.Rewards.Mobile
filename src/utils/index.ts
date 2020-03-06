 import {createElement} from 'react';

 export const fetchData = async <T>(command: Function): Promise<T | void> => {
  try {
    const response = await command();
    if (response) {
      return response;
    }
  } catch (e) {
    console.log("something went wrong", e);
  }
};

export const renderComponentXTimes = (component: any, x : number ,props?: any,children?: any) => {
  return Array.from({length: x}, (k, i) => createElement(component,{...props,key: i}, children));
}
