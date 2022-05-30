import { useState, useEffect, useRef } from 'react';
import { BaseClient } from 'services';
import { State } from 'store';

export const usePortal = (id: string) => {
  const rootElemRef = useRef(document.createElement('div'));

  useEffect(() => {
    const parentElem = document.querySelector(`#${id}`);
    if (parentElem) {
      parentElem.appendChild(rootElemRef.current);
    }
    return () => {
      rootElemRef.current.remove();
    };
  }, []);
  return rootElemRef.current;
};

export const useAuthenticatedClient = <T>(client: BaseClient, token?: string) => {
  const [authenticatedClient, setAuthenticatedClient] = useState();

  useEffect(() => {
    if (!client && !token) {
      return;
    }
    if (client.token) {
      setAuthenticatedClient(client);
    } else {
      if (token) {
        client.setAuthToken(token);
        setAuthenticatedClient(client);
      }
    }
  }, [client, token]);

  return authenticatedClient;
};
