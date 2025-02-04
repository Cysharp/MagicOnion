import React, {type ReactNode} from 'react';
import clsx from 'clsx';
import MDXContent from '@theme-original/MDXContent';
import type MDXContentType from '@theme/MDXContent';
import type {WrapperProps} from '@docusaurus/types';
import styles from './styles.module.css';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';

type Props = WrapperProps<typeof MDXContentType>;

export default function MDXContentWrapper(props: Props): ReactNode {
  const {i18n} = useDocusaurusContext();
  return (
    <>
      {(i18n.currentLocale != 'ja' && i18n.currentLocale != 'en') &&
        <div className={clsx(styles.communityNotice, "alert alert--secondary")}>
          Translation of pages other than English and Japanese versions is maintained by community contributions.
          The project does not guarantee the accuracy of the content and may not reflect the latest content.
        </div>
      }
      <MDXContent {...props} />
    </>
  );
}
