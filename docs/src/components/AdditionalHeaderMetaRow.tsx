import React from 'react';

import LastUpdated from '@theme/LastUpdated';
import {useDoc} from '@docusaurus/plugin-content-docs/client'

export default function AdditionalHeaderMetaRow(props: { children: React.ReactNode }) {
  const {metadata} = useDoc();

  return (
    <>
      <LastUpdated lastUpdatedAt={metadata.lastUpdatedAt} />
    </>
  );
}
